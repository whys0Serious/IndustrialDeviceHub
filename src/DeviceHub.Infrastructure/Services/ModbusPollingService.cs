using DeviceHub.Core.DTOs;
using DeviceHub.Core.Enums;
using DeviceHub.Core.Interfaces;
using DeviceHub.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Infrastructure.Services
{
    /// <summary>
    /// Modbus后台轮询服务：每秒读寄存器，写入缓存
    /// </summary>
    public class ModbusPollingService : BackgroundService
    {
        private readonly IDeviceCommunication _communication;
        private readonly IRealtimeDataCache _cache;
        private readonly ModbusOptions _options;
        private readonly ILogger<ModbusPollingService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AlarmOptions _alarmOptions;
        private readonly AlarmTracker _alarmTracker;


        public ModbusPollingService(
            IDeviceCommunication communication,
            IRealtimeDataCache cache,
            IOptions<ModbusOptions> options,
            ILogger<ModbusPollingService> logger,
            IServiceScopeFactory scopeFactory,
            IOptions<AlarmOptions> alarmOptions,
            AlarmTracker alarmTracker)
        {
            _communication = communication;
            _cache = cache;
            _options = options.Value;
            _logger = logger;
            _scopeFactory = scopeFactory;
            _alarmOptions = alarmOptions.Value;
            _alarmTracker = alarmTracker;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("Modbus 轮询已禁用");
                return;
            }

            _logger.LogInformation("Modbus 轮询服务启动：{Host}:{Port}, 每 {Interval}ms 一次",
                _options.Host, _options.Port, _options.PollingIntervalMs);

            await EnsureConnectedAsync(stoppingToken);

            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_options.PollingIntervalMs));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //断线重连
                    if (!_communication.IsConnected)
                    {
                        _logger.LogWarning("Modbus 连接断开，尝试重连...");
                        await EnsureConnectedAsync(stoppingToken);
                    }

                    if (_communication.IsConnected)
                    {
                        await PollOnceAsync(stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Modbus 轮询发生异常，将在下一轮尝试重连");

                    //主动断开，让IsConnected变 false
                    try
                    {
                        await _communication.DisconnectAsync();
                    }
                    catch { } //忽略断开异常
                }

                try
                {
                    await timer.WaitForNextTickAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("Modbus 轮询服务已停止");
            await _communication.DisconnectAsync();
        }

        /// <summary>
        /// 确保连接（含重试）
        /// </summary>
        private async Task EnsureConnectedAsync(CancellationToken ct)
        {
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                var connected = await _communication.ConnectAsync(_options.Host, _options.Port, ct);
                if (connected) return;

                _logger.LogWarning("第 {Attempt} 次连接失败，3 秒后重试", attempt);
                await Task.Delay(3000, ct);
            }

            _logger.LogError("Modbus 连接失败（已重试 3 次）");
        }

        /// <summary>
        /// 单次轮询
        /// </summary>
        private async Task PollOnceAsync(CancellationToken ct)
        {
            var registers = await _communication.ReadHoldingRegistersAsync(
                _options.SlaveId,
                _options.StartAddress,
                _options.RegisterCount,
                ct);

            // 解析数据（示例：地址 0 = 温度，1 = 压力，2 = 转速）
            // 用一个固定的"虚拟设备 Id" = 1（M3 先做单设备，M4 再扩多设备）
            var data = new RealtimeDataDto
            {
                DeviceId = 1,
                Temperature = registers.Length > 0 ? registers[0] / 10.0 : 0,   //250 → 25.0℃
                Pressure = registers.Length > 1 ? registers[1] : 0,
                Speed = registers.Length > 2 ? registers[2] : 0,
                Timestamp = DateTime.Now
            };

            _cache.Update(data.DeviceId, data);

            //状态变化触发报警
            await CheckAlarmsAsync(data, ct);

            _logger.LogTrace("采集：温度={Temp}℃, 压力={Press}kPa, 转速={Speed}RPM",
                data.Temperature, data.Pressure, data.Speed);
        }

        private async Task CheckAlarmsAsync(RealtimeDataDto data, CancellationToken ct)
        {
            try
            {
                // 温度过高
                await CheckOneAsync(
                    data, AlarmType.TemperatureHigh,
                    data.Temperature > _alarmOptions.TemperatureMax,
                    data.Temperature, _alarmOptions.TemperatureMax,
                    $"温度过高：当前 {data.Temperature:F1}℃，超过上限 {_alarmOptions.TemperatureMax:F1}℃",
                    ct);

                // 压力过高
                await CheckOneAsync(
                    data, AlarmType.PressureHigh,
                    data.Pressure > _alarmOptions.PressureMax,
                    data.Pressure, _alarmOptions.PressureMax,
                    $"压力过高：当前 {data.Pressure:F0}kPa，超过上限 {_alarmOptions.PressureMax:F0}kPa",
                    ct);

                // 转速过低
                await CheckOneAsync(
                    data, AlarmType.SpeedLow,
                    data.Speed < _alarmOptions.SpeedMin,
                    data.Speed, _alarmOptions.SpeedMin,
                    $"转速过低：当前 {data.Speed}RPM，低于下限 {_alarmOptions.SpeedMin:F0}RPM",
                    ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "报警判断异常");
            }
        }
        private async Task CheckOneAsync(
              RealtimeDataDto data, AlarmType type,
              bool isActive, double currentValue, double threshold, string message,
              CancellationToken ct)
        {
            var (triggered, resolved) = _alarmTracker.Update(data.DeviceId, type, isActive);

            if (!triggered && !resolved)
                return;   //无状态变化跳过

            using var scope = _scopeFactory.CreateScope();
            var alarmService = scope.ServiceProvider.GetRequiredService<IAlarmService>();

            if (triggered)
            {
                //首次触发 写新报警
                await alarmService.CreateAsync(
                    data.DeviceId, type, message,
                    currentValue, threshold, ct);
            }
            else if (resolved)
            {
                //恢复 更新报警记录
                await alarmService.ResolveAsync(data.DeviceId, type, ct);
            }
        }
    }
}

using DeviceHub.Core.DTOs;
using DeviceHub.Core.Interfaces;
using DeviceHub.Core.Models;
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

        public ModbusPollingService(
            IDeviceCommunication communication,
            IRealtimeDataCache cache,
            IOptions<ModbusOptions> options,
            ILogger<ModbusPollingService> logger)
        {
            _communication = communication;
            _cache = cache;
            _options = options.Value;
            _logger = logger;
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

            _logger.LogTrace("采集：温度={Temp}℃, 压力={Press}kPa, 转速={Speed}RPM",
                data.Temperature, data.Pressure, data.Speed);
        }
    }
}

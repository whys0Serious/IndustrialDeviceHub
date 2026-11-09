using DeviceHub.Core.DTOs;
using DeviceHub.Core.Enums;
using DeviceHub.Core.Interfaces;
using DeviceHub.Core.Models;
using DeviceHub.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DeviceHub.Infrastructure.BackgroundServices;

/// <summary>
/// Modbus 轮询服务
/// </summary>
public class ModbusPollingService : BackgroundService
{
    private readonly IDeviceCommunication _communication;
    private readonly IRealtimeDataCache _cache;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AlarmTracker _alarmTracker;
    private readonly ModbusOptions _options;
    private readonly AlarmOptions _alarmOptions;
    private readonly ILogger<ModbusPollingService> _logger;

    public ModbusPollingService(
        IDeviceCommunication communication,
        IRealtimeDataCache cache,
        IServiceScopeFactory scopeFactory,
        AlarmTracker alarmTracker,
        IOptions<ModbusOptions> options,
        IOptions<AlarmOptions> alarmOptions,
        ILogger<ModbusPollingService> logger)
    {
        _communication = communication;
        _cache = cache;
        _scopeFactory = scopeFactory;
        _alarmTracker = alarmTracker;
        _options = options.Value;
        _alarmOptions = alarmOptions.Value;
        _logger = logger;
    }
    //死区设定
    private const double TempDeadband = 2.0;    // 温度回落2度才恢复
    private const double PressDeadband = 20.0;  // 压力回落20kPa才恢复
    private const double SpeedDeadband = 50.0;  // 转速回升50RPM才恢复
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //立即让出，Host不等待本服务的首次连接
        await Task.Yield();
        if (!_options.Enabled)
        {
            _logger.LogInformation("Modbus 轮询已禁用");
            return;
        }

        _logger.LogInformation("Modbus 轮询服务启动：{Host}:{Port}, 每 {Interval}ms 一次",
            _options.Host, _options.Port, _options.PollingIntervalMs);

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_options.PollingIntervalMs));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                //每次循环只尝试一次连接，失败等下个tick
                if (!_communication.IsConnected)
                {
                    await TryConnectOnceAsync(stoppingToken);
                }

                if (_communication.IsConnected)
                {
                    await PollAllDevicesAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Modbus 轮询发生异常，将在下一轮尝试重连");
                try { await _communication.DisconnectAsync(); } catch { }
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
    /// 尝试一次连接；失败不阻塞，交由下一个 tick 再试。
    /// 仿真器未启动时，客户端仍能正常进入主界面。
    /// </summary>
    private async Task TryConnectOnceAsync(CancellationToken ct)
    {
        try
        {
            var connected = await _communication.ConnectAsync(_options.Host, _options.Port, ct);
            if (connected)
            {
                _logger.LogInformation("Modbus 已连接 {Host}:{Port}", _options.Host, _options.Port);
            }
            else
            {
                _logger.LogDebug("Modbus 连接失败，下个周期重试");
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Modbus 连接异常，下个周期重试");
        }
    }

    /// <summary>
    /// 遍历所有启用监控的设备
    /// </summary>
    private async Task PollAllDevicesAsync(CancellationToken ct)
    {
        //每次轮询创建新Scope 避免DbContext冲突
        using var scope = _scopeFactory.CreateScope();
        var deviceRepo = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();

        var devices = await deviceRepo.GetMonitoredDevicesAsync(ct);

        if (devices.Count == 0)
        {
            _logger.LogTrace("没有启用监控的设备");
            return;
        }

        foreach (var device in devices)
        {
            await PollDeviceAsync(device, ct);
        }
    }

    /// <summary>
    /// 采集单台设备
    /// </summary>
    private async Task PollDeviceAsync(Core.Entities.Device device, CancellationToken ct)
    {
        if (!device.ModbusSlaveId.HasValue) return;

        try
        {
            var registers = await _communication.ReadHoldingRegistersAsync(
                device.ModbusSlaveId.Value,
                device.ModbusStartAddress ?? 0,
                device.ModbusRegisterCount ?? 10,
                ct);

            var data = new RealtimeDataDto
            {
                DeviceId = device.Id,
                Temperature = registers.Length > 0 ? registers[0] / 10.0 : 0,
                Pressure = registers.Length > 1 ? registers[1] : 0,
                Speed = registers.Length > 2 ? registers[2] : 0,
                Timestamp = DateTime.Now
            };

            _cache.Update(data.DeviceId, data);

            await CheckAlarmsAsync(data, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "采集设备 {DeviceId} ({Name}) 失败",
                device.Id, device.Name);
        }
    }

    private async Task CheckAlarmsAsync(RealtimeDataDto data, CancellationToken ct)
    {
        try
        {
            await CheckOneAsync(
                data, AlarmType.TemperatureHigh,
                data.Temperature > _alarmOptions.TemperatureMax,
                data.Temperature, _alarmOptions.TemperatureMax,
                TempDeadband,true,
                $"温度过高：当前 {data.Temperature:F1}℃，超过上限 {_alarmOptions.TemperatureMax:F1}℃",
                ct);

            await CheckOneAsync(
                data, AlarmType.PressureHigh,
                data.Pressure > _alarmOptions.PressureMax,
                data.Pressure, _alarmOptions.PressureMax,
                PressDeadband,true,
                $"压力过高：当前 {data.Pressure:F0}kPa，超过上限 {_alarmOptions.PressureMax:F0}kPa",
                ct);

            await CheckOneAsync(
                data, AlarmType.SpeedLow,
                data.Speed > 0 && data.Speed < _alarmOptions.SpeedMin,
                data.Speed, _alarmOptions.SpeedMin,
                SpeedDeadband, false,
                $"转速过低：当前 {data.Speed}RPM，低于下限 {_alarmOptions.SpeedMin:F0}RPM",
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "报警判断异常：DeviceId={DeviceId}", data.DeviceId);
        }
    }

    /// <summary>
    /// 检查单个报警并处理死区逻辑
    /// </summary>
    private async Task CheckOneAsync(
        RealtimeDataDto data, AlarmType type,
        bool isExceeded, double currentValue, double threshold,
        double deadband, bool isHighAlarm, string message,
        CancellationToken ct)
    {
        //获取上一次的状态机结果
        var (triggered, resolved) = _alarmTracker.Update(data.DeviceId, type, isExceeded);

        //引入死区（防抖）处理
        // 如果目前状态机判定为“恢复”（即从 true 变 false）
        if (resolved)
        {
            // 判断是否真的脱离了死区
            bool inDeadband = isHighAlarm
                ? currentValue > (threshold - deadband)  // 如果是高限报警：当前值如果在 (阈值-死区) 之上，说明还在死区内
                : currentValue < (threshold + deadband); // 如果是低限报警：当前值如果在 (阈值+死区) 之下，说明还在死区内

            if (inDeadband)
            {
                // 值还在死区内（虽然没超标，但也没完全恢复），强行把状态机改回触发状态，并且拦截恢复
                _alarmTracker.Update(data.DeviceId, type, true);
                return;
            }
        }

        if (!triggered && !resolved) return;

        using var scope = _scopeFactory.CreateScope();
        var alarmService = scope.ServiceProvider.GetRequiredService<IAlarmService>();

        if (triggered)
        {
            await alarmService.CreateAsync(data.DeviceId, type, message, currentValue, threshold, ct);
        }
        else if (resolved)
        {
            await alarmService.ResolveAsync(data.DeviceId, type, ct);
        }
    }
}
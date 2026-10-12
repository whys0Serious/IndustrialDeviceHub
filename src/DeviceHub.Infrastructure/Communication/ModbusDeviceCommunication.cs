using DeviceHub.Core.Interfaces;
using Microsoft.Extensions.Logging;
using NModbus;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace DeviceHub.Infrastructure.Communication
{
    /// <summary>
    /// Modbus TCP通信实现
    /// </summary>
    public class ModbusDeviceCommunication : IDeviceCommunication
    {
        private readonly ILogger<ModbusDeviceCommunication> _logger;

        private TcpClient? _tcpClient;
        private IModbusMaster? _master;

        public ModbusDeviceCommunication(ILogger<ModbusDeviceCommunication> logger)
        {
            _logger = logger;
        }

        public bool IsConnected => _tcpClient?.Connected == true;

        public async Task<bool> ConnectAsync(string host, int port, CancellationToken ct = default)
        {
            try
            {
                //只在有旧连接时才断开（幂等快速）
                if (_tcpClient != null || _master != null)
                {
                    await DisconnectAsync(ct);
                }

                _tcpClient = new TcpClient();

                //带超时的连接
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

                await _tcpClient.ConnectAsync(host, port, timeoutCts.Token);

                var factory = new ModbusFactory();
                _master = factory.CreateMaster(_tcpClient);

                //设置超时
                _master.Transport.ReadTimeout = 3000;
                _master.Transport.WriteTimeout = 3000;
                _master.Transport.Retries = 2;

                _logger.LogInformation("Modbus 连接成功：{Host}:{Port}", host, port);
                _firstFailureLogged = false;   //成功后重置失败标记
                return true;
            }
            catch (OperationCanceledException)
            {
                LogConnectFailure(host, port, isTimeout: true);
                return false;
            }
            catch (Exception ex)
            {
                LogConnectFailure(host, port, isTimeout: false, ex);
                return false;
            }
        }
        private bool _firstFailureLogged = false;

        private void LogConnectFailure(string host, int port, bool isTimeout, Exception? ex = null)
        {
            var msg = isTimeout
                ? "Modbus 连接超时：{Host}:{Port}"
                : "Modbus 连接失败：{Host}:{Port}";

            if (!_firstFailureLogged)
            {
                _logger.LogWarning(ex, msg + "（后续失败将降为 Debug）", host, port);
                _firstFailureLogged = true;
            }
            else
            {
                _logger.LogDebug(ex, msg, host, port);
            }
        }
        public Task DisconnectAsync(CancellationToken ct = default)
        {
            var wasConnected = _tcpClient != null || _master != null;

            //每个资源独立 try-catch，保证尽最大释放全部
            try { _master?.Dispose(); }
            catch (Exception ex) { _logger.LogWarning(ex, "释放 Modbus Master 异常"); }

            try { _tcpClient?.Close(); }
            catch (Exception ex) { _logger.LogWarning(ex, "关闭 TCP 连接异常"); }

            try { _tcpClient?.Dispose(); }
            catch (Exception ex) { _logger.LogWarning(ex, "释放 TCP 客户端异常"); }

            _master = null;
            _tcpClient = null;

            //只在确实断开过时打日志，避免刷屏
            if (wasConnected)
            {
                _logger.LogInformation("Modbus 已断开连接");
            }

            return Task.CompletedTask;
        }

        public async Task<ushort[]> ReadHoldingRegistersAsync(
            byte slaveId, ushort startAddress, ushort count, CancellationToken ct = default)
        {
            if (_master == null || !IsConnected)
                throw new InvalidOperationException("Modbus 未连接");

            try
            {
                return await _master.ReadHoldingRegistersAsync(slaveId, startAddress, count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "读取失败：slave={SlaveId}, addr={Addr}", slaveId, startAddress);

                //连接出问题，主动清理，让上层触发重连
                try { await DisconnectAsync(ct); } catch { }

                throw;
            }
        }

        public async Task WriteSingleRegisterAsync(
            byte slaveId,
            ushort address,
            ushort value,
            CancellationToken ct = default)
        {
            if (_master == null || !IsConnected)
                throw new InvalidOperationException("Modbus 未连接，请先调用 ConnectAsync");

            try
            {
                await _master.WriteSingleRegisterAsync(slaveId, address, value);
                _logger.LogInformation("写入寄存器：slave={SlaveId}, addr={Addr}, value={Value}",
                    slaveId, address, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "写入寄存器失败：slave={SlaveId}, addr={Addr}",
                    slaveId, address);
                throw;
            }
        }

        public void Dispose()
        {
            DisconnectAsync().GetAwaiter().GetResult();
        }
    }
}

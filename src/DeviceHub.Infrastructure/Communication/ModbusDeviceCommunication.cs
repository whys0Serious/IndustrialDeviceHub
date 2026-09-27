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
                //先断开旧连接
                await DisconnectAsync(ct);

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
                return true;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Modbus 连接超时：{Host}:{Port}", host, port);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Modbus 连接失败：{Host}:{Port}", host, port);
                return false;
            }
        }

        public Task DisconnectAsync(CancellationToken ct = default)
        {
            try
            {
                _master?.Dispose();
                _tcpClient?.Close();
                _tcpClient?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Modbus 断开连接时发生异常");
            }
            finally
            {
                _master = null;
                _tcpClient = null;
            }

            _logger.LogInformation("Modbus 已断开连接");
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

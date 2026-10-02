using DeviceHub.Infrastructure.Communication;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace DeviceHub.Tests.Communication
{
    public class ModbusCommunicationTests
    {
        private readonly ITestOutputHelper _output;

        public ModbusCommunicationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact(Skip = "需要先启动 DeviceHub.Simulator（监听 502）")]
        public async Task Connect_And_Read_Registers_Should_Work()
        {
            
            var comm = new ModbusDeviceCommunication(
                NullLogger<ModbusDeviceCommunication>.Instance);

            
            var connected = await comm.ConnectAsync("127.0.0.1", 502);
            _output.WriteLine($"Connected: {connected}");

            Assert.True(connected, "Modbus 连接失败，请先启动仿真器（监听 127.0.0.1:502）");

            //读10个保持寄存器
            var registers = await comm.ReadHoldingRegistersAsync(
                slaveId: 1,
                startAddress: 0,
                count: 10);

            _output.WriteLine($"读取到 {registers.Length} 个寄存器：");
            for (int i = 0; i < registers.Length; i++)
            {
                _output.WriteLine($"  地址 {i}: {registers[i]}");
            }

            Assert.Equal(10, registers.Length);

            //清理
            await comm.DisconnectAsync();
        }
    }
}

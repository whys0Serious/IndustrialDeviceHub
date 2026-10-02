using NModbus;
using NModbus.Data;
using System.Net;
using System.Net.Sockets;

//配置
var devices = new[]
{
    new { SlaveId = (byte)1, Name = "温度传感器1",  BaseTemp = 250, BasePress = 0,   BaseSpeed = 0 },
    new { SlaveId = (byte)2, Name = "压力传感器1",  BaseTemp = 0,   BasePress = 100, BaseSpeed = 0 },
    new { SlaveId = (byte)3, Name = "1号主电机",    BaseTemp = 450, BasePress = 0,   BaseSpeed = 1200 }
};

Console.WriteLine("=========================================");
Console.WriteLine("  Modbus TCP 仿真器");
Console.WriteLine("  监听：0.0.0.0:502");
Console.WriteLine("=========================================");
Console.WriteLine();
Console.WriteLine("模拟设备：");
foreach (var d in devices)
    Console.WriteLine($"  从站 {d.SlaveId}: {d.Name}");
Console.WriteLine();
Console.WriteLine("按 Ctrl+C 退出");
Console.WriteLine();

//启动TCP监听
var listener = new TcpListener(IPAddress.Any, 502);
listener.Start();

var factory = new ModbusFactory();
var slaveNetwork = factory.CreateSlaveNetwork(listener);

//为每台设备创建从站
var slaves = new List<(byte SlaveId, ISlaveDataStore Store, ushort BaseTemp, ushort BasePress, ushort BaseSpeed)>();

foreach (var dev in devices)
{
    var dataStore = new DefaultSlaveDataStore();

    //初始化10个保持寄存器
    for (ushort i = 0; i < 10; i++)
        dataStore.HoldingRegisters.WritePoints(i, new ushort[] { 0 });

    var slave = factory.CreateSlave(dev.SlaveId, dataStore);
    slaveNetwork.AddSlave(slave);

    slaves.Add((dev.SlaveId, dataStore,
                (ushort)dev.BaseTemp, (ushort)dev.BasePress, (ushort)dev.BaseSpeed));
}

//后台线程：每秒更新寄存器
_ = Task.Run(async () =>
{
    var random = new Random();

    while (true)
    {
        foreach (var s in slaves)
        {
            //温度
            var temp = s.BaseTemp > 0
                ? (ushort)(s.BaseTemp + random.Next(-20, 21))
                : (ushort)0;
            s.Store.HoldingRegisters.WritePoints(0, new ushort[] { temp });

            //压力
            var press = s.BasePress > 0
                ? (ushort)(s.BasePress + random.Next(-15, 16))
                : (ushort)0;
            s.Store.HoldingRegisters.WritePoints(1, new ushort[] { press });

            //转速
            var speed = s.BaseSpeed > 0
                ? (ushort)(s.BaseSpeed + random.Next(-50, 51))
                : (ushort)0;
            s.Store.HoldingRegisters.WritePoints(2, new ushort[] { speed });
        }

        await Task.Delay(1000);
    }
});

//启动
await slaveNetwork.ListenAsync();

await Task.Delay(Timeout.Infinite);
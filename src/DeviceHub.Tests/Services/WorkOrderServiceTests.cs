using DeviceHub.Core.DTOs;
using DeviceHub.Core.Entities;
using DeviceHub.Core.Enums;
using DeviceHub.Core.Exceptions;
using DeviceHub.Core.Interfaces;
using DeviceHub.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DeviceHub.Tests.Services;

public class WorkOrderServiceTests
{
    private readonly Mock<IWorkOrderRepository> _orderRepoMock;
    private readonly Mock<IDeviceRepository> _deviceRepoMock;
    private readonly Mock<ILogger<WorkOrderService>> _loggerMock;
    private readonly WorkOrderService _service;

    public WorkOrderServiceTests()
    {
        _orderRepoMock = new Mock<IWorkOrderRepository>();
        _deviceRepoMock = new Mock<IDeviceRepository>();
        _loggerMock = new Mock<ILogger<WorkOrderService>>();
        _service = new WorkOrderService(
            _orderRepoMock.Object,
            _deviceRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_Should_Create_Order_With_Pending_Status()
    {
        var request = new CreateWorkOrderRequest
        {
            DeviceId = 1,
            Title = "测试工单",
            Description = "测试描述",
            Priority = WorkOrderPriority.High,
            CreatedBy = "admin"
        };

        _deviceRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Device { Id = 1, Name = "测试设备" });

        _orderRepoMock
            .Setup(r => r.GenerateOrderNoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("WO-20261002-001");

        _orderRepoMock
            .Setup(r => r.AddAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkOrder o, CancellationToken _) =>
            {
                o.Id = 100;
                return o;
            });

        var id = await _service.CreateAsync(request);

        id.Should().Be(100);
        _orderRepoMock.Verify(r => r.AddAsync(
            It.Is<WorkOrder>(o =>
                o.DeviceId == 1 &&
                o.Title == "测试工单" &&
                o.Status == WorkOrderStatus.Pending &&
                o.Priority == WorkOrderPriority.High &&
                o.CreatedBy == "admin"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_When_Device_Not_Found_Should_Throw()
    {
        var request = new CreateWorkOrderRequest { DeviceId = 999, Title = "T", Description = "D" };

        _deviceRepoMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Device?)null);

        var act = async () => await _service.CreateAsync(request);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*设备*不存在*");
    }


    [Fact]
    public async Task StartAsync_From_Pending_Should_Transition_To_Processing()
    {
        var order = new WorkOrder
        {
            Id = 1,
            Status = WorkOrderStatus.Pending
        };

        _orderRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        await _service.StartAsync(1, "engineer1");

        order.Status.Should().Be(WorkOrderStatus.Processing);
        order.AssignedTo.Should().Be("engineer1");
        order.StartedAt.Should().NotBeNull();

        _orderRepoMock.Verify(r => r.UpdateAsync(
            It.Is<WorkOrder>(o => o.Status == WorkOrderStatus.Processing),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task StartAsync_From_Closed_Should_Throw()
    {
        var order = new WorkOrder
        {
            Id = 1,
            Status = WorkOrderStatus.Closed
        };

        _orderRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var act = async () => await _service.StartAsync(1, "engineer1");

        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task StartAsync_From_Processing_Should_Throw()
    {
        var order = new WorkOrder
        {
            Id = 1,
            Status = WorkOrderStatus.Processing
        };

        _orderRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var act = async () => await _service.StartAsync(1, "engineer1");

        await act.Should().ThrowAsync<BusinessException>();
    }


    [Fact]
    public async Task CloseAsync_From_Processing_Should_Transition_To_Closed()
    {
        var order = new WorkOrder
        {
            Id = 1,
            Status = WorkOrderStatus.Processing,
            AssignedTo = "engineer1"
        };

        _orderRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        await _service.CloseAsync(1, "更换散热风扇");

        order.Status.Should().Be(WorkOrderStatus.Closed);
        order.Resolution.Should().Be("更换散热风扇");
        order.ClosedAt.Should().NotBeNull();

        _orderRepoMock.Verify(r => r.UpdateAsync(
            It.Is<WorkOrder>(o => o.Status == WorkOrderStatus.Closed),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CloseAsync_From_Pending_Should_Throw()
    {
        var order = new WorkOrder
        {
            Id = 1,
            Status = WorkOrderStatus.Pending
        };

        _orderRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var act = async () => await _service.CloseAsync(1, "test");

        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task CloseAsync_From_Closed_Should_Throw()
    {
        var order = new WorkOrder
        {
            Id = 1,
            Status = WorkOrderStatus.Closed
        };

        _orderRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var act = async () => await _service.CloseAsync(1, "test");

        await act.Should().ThrowAsync<BusinessException>();
    }

    /// <summary>
    /// UpdateAsync
    /// </summary>
    /// <returns></returns>

    [Fact]
    public async Task UpdateAsync_When_Closed_Should_Throw()
    {
        var order = new WorkOrder
        {
            Id = 1,
            Status = WorkOrderStatus.Closed
        };

        _orderRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var request = new UpdateWorkOrderRequest
        {
            Title = "新标题",
            Description = "新描述",
            Priority = WorkOrderPriority.High
        };

        var act = async () => await _service.UpdateAsync(1, request);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*已关闭*不能修改*");
    }


    [Fact]
    public async Task GetLogsAsync_Should_Return_Logs_Descending()
    {
        var order = new WorkOrder
        {
            Id = 1,
            Logs = new List<WorkOrderLog>
            {
                new() { Id = 1, Action = "创建工单", CreatedAt = DateTime.Now.AddMinutes(-10) },
                new() { Id = 2, Action = "开始处理", CreatedAt = DateTime.Now.AddMinutes(-5) },
                new() { Id = 3, Action = "关闭工单", CreatedAt = DateTime.Now }
            }
        };

        _orderRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var logs = await _service.GetLogsAsync(1);

        logs.Should().HaveCount(3);
        logs[0].Action.Should().Be("关闭工单");   // 最新的在前
        logs[2].Action.Should().Be("创建工单");
    }
}
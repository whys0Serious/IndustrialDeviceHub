using DeviceHub.Core.Enums;
using DeviceHub.Core.Exceptions;
using DeviceHub.Core.Services;
using FluentAssertions;
using Xunit;

namespace DeviceHub.Tests.Services;

public class WorkOrderStateMachineTests
{
    /// <summary>
    /// CanTransition 所有合法路径
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="expected"></param>

    [Theory]
    [InlineData(WorkOrderStatus.Pending, WorkOrderStatus.Processing, true)]
    [InlineData(WorkOrderStatus.Processing, WorkOrderStatus.Closed, true)]
    [InlineData(WorkOrderStatus.Pending, WorkOrderStatus.Closed, false)]      //跳步
    [InlineData(WorkOrderStatus.Processing, WorkOrderStatus.Pending, false)]  //回退
    [InlineData(WorkOrderStatus.Closed, WorkOrderStatus.Pending, false)]      //已关闭不能变
    [InlineData(WorkOrderStatus.Closed, WorkOrderStatus.Processing, false)]
    [InlineData(WorkOrderStatus.Closed, WorkOrderStatus.Closed, false)]       //自己到自己
    [InlineData(WorkOrderStatus.Pending, WorkOrderStatus.Pending, false)]
    [InlineData(WorkOrderStatus.Processing, WorkOrderStatus.Processing, false)]
    public void CanTransition_Should_Return_Expected(
        WorkOrderStatus from, WorkOrderStatus to, bool expected)
    {
        var result = WorkOrderStateMachine.CanTransition(from, to);

        result.Should().Be(expected);
    }

    /// <summary>
    /// EnsureCanTransition合法时不抛
    /// </summary>

    [Fact]
    public void EnsureCanTransition_PendingToProcessing_Should_Not_Throw()
    {
        var act = () => WorkOrderStateMachine.EnsureCanTransition(
            WorkOrderStatus.Pending, WorkOrderStatus.Processing);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureCanTransition_ProcessingToClosed_Should_Not_Throw()
    {
        var act = () => WorkOrderStateMachine.EnsureCanTransition(
            WorkOrderStatus.Processing, WorkOrderStatus.Closed);

        act.Should().NotThrow();
    }

    /// <summary>
    /// EnsureCanTransition 非法时抛异常
    /// </summary>

    [Fact]
    public void EnsureCanTransition_PendingToClosed_Should_Throw()
    {
        var act = () => WorkOrderStateMachine.EnsureCanTransition(
            WorkOrderStatus.Pending, WorkOrderStatus.Closed);

        act.Should().Throw<BusinessException>()
           .WithMessage("*待处理*已关闭*");
    }

    [Fact]
    public void EnsureCanTransition_ClosedToProcessing_Should_Throw()
    {
        var act = () => WorkOrderStateMachine.EnsureCanTransition(
            WorkOrderStatus.Closed, WorkOrderStatus.Processing);

        act.Should().Throw<BusinessException>();
    }

    [Fact]
    public void EnsureCanTransition_ClosedToPending_Should_Throw()
    {
        var act = () => WorkOrderStateMachine.EnsureCanTransition(
            WorkOrderStatus.Closed, WorkOrderStatus.Pending);

        act.Should().Throw<BusinessException>();
    }

    [Theory]
    [InlineData(WorkOrderStatus.Pending, "待处理")]
    [InlineData(WorkOrderStatus.Processing, "处理中")]
    [InlineData(WorkOrderStatus.Closed, "已关闭")]
    public void GetStatusText_Should_Return_ChineseText(
        WorkOrderStatus status, string expected)
    {
        var result = WorkOrderStateMachine.GetStatusText(status);

        result.Should().Be(expected);
    }

    [Fact]
    public void GetStatusText_Unknown_Should_Return_Unknown()
    {
        var result = WorkOrderStateMachine.GetStatusText((WorkOrderStatus)999);

        result.Should().Be("未知");
    }
}
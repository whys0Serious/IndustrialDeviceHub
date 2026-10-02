using DeviceHub.Core.DTOs;
using DeviceHub.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DeviceHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkOrdersController : ControllerBase
{
    private readonly IWorkOrderService _workOrderService;

    public WorkOrdersController(IWorkOrderService workOrderService)
    {
        _workOrderService = workOrderService;
    }

    /// <summary>
    /// 工单列表（分页 + 筛选）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? keyword = null,
        [FromQuery] int? status = null,
        [FromQuery] int? priority = null,
        [FromQuery] int? deviceId = null,
        CancellationToken ct = default)
    {
        var query = new WorkOrderQueryParams
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            Keyword = keyword,
            Status = status.HasValue ? (Core.Enums.WorkOrderStatus)status.Value : null,
            Priority = priority.HasValue ? (Core.Enums.WorkOrderPriority)priority.Value : null,
            DeviceId = deviceId
        };

        var result = await _workOrderService.GetPagedAsync(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// 工单详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var order = await _workOrderService.GetByIdAsync(id, ct);
        return order == null ? NotFound() : Ok(order);
    }

    /// <summary>
    /// 工单处理记录
    /// </summary>
    [HttpGet("{id}/logs")]
    public async Task<IActionResult> GetLogs(int id, CancellationToken ct = default)
    {
        var logs = await _workOrderService.GetLogsAsync(id, ct);
        return Ok(logs);
    }
}
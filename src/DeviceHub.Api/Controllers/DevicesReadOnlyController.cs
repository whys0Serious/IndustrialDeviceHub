using DeviceHub.Core.DTOs;
using DeviceHub.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DeviceHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;

    public DevicesController(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    /// <summary>
    /// 设备列表（分页）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? keyword = null,
        CancellationToken ct = default)
    {
        var query = new DeviceQueryDto
        {
            Page = pageIndex,
            PageSize = pageSize,
            Keyword = keyword
        };

        var result = await _deviceService.GetPagedAsync(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// 设备详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var device = await _deviceService.GetByIdAsync(id, ct);
        return device == null ? NotFound() : Ok(device);
    }

    /// <summary>
    /// 设备统计
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(CancellationToken ct = default)
    {
        var stats = await _deviceService.GetStatisticsAsync(ct);
        return Ok(stats);
    }
}
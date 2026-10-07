using DeviceHub.Core.DTOs;
using DeviceHub.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DeviceHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;

    public AiController(IAiService aiService)
    {
        _aiService = aiService;
    }

    /// <summary>
    /// AI对话
    /// </summary>
    [HttpPost("chat")]
    public async Task<IActionResult> Chat(
        [FromBody] AiChatRequest request,
        CancellationToken ct)
    {
        var reply = await _aiService.ChatAsync(request.Message, ct);
        return Ok(new { reply });
    }

    /// <summary>
    /// 测试LLM连接
    /// </summary>
    [HttpGet("test")]
    public async Task<IActionResult> Test(CancellationToken ct)
    {
        var ok = await _aiService.TestConnectionAsync(ct);
        return Ok(new { connected = ok });
    }
}
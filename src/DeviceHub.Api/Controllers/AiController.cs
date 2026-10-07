using DeviceHub.Core.DTOs;
using DeviceHub.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DeviceHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;
    private readonly IAiKnowledgeService _knowledgeService;

    public AiController(IAiService aiService, IAiKnowledgeService knowledgeService)
    {
        _aiService = aiService;
        _knowledgeService = knowledgeService;
    }

    /// <summary>AI对（简单对话）</summary>
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] AiChatRequest request, CancellationToken ct)
    {
        var reply = await _aiService.ChatAsync(request.Message, ct);
        return Ok(new { reply });
    }

    /// <summary>
    /// AI故障诊断（检索相似案例+生成建议）
    /// </summary>
    [HttpPost("diagnose")]
    public async Task<IActionResult> Diagnose([FromBody] AiChatRequest request, CancellationToken ct)
    {
        var result = await _knowledgeService.SearchAndSuggestAsync(request.Message, 3, ct);
        return Ok(result);
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
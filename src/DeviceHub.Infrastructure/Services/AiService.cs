using DeviceHub.Core.Exceptions;
using DeviceHub.Core.Interfaces;
using DeviceHub.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace DeviceHub.Infrastructure.Services;

/// <summary>
/// AI服务实现（Semantic Kernel）
/// </summary>
public class AiService : IAiService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    private readonly AiOptions _options;
    private readonly ILogger<AiService> _logger;

    public AiService(
        Kernel kernel,
        IOptions<AiOptions> options,
        ILogger<AiService> logger)
    {
        _kernel = kernel;
        _options = options.Value;
        _logger = logger;

        //从Kernel拿ChatCompletionService
        _chatService = kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<string> ChatAsync(string message, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new BusinessException("消息不能为空");

        try
        {
            var history = new ChatHistory();
            history.AddSystemMessage("你是一个工业设备管理助手，帮助用户分析设备故障、推荐处理方案。回答简洁、专业。");
            history.AddUserMessage(message);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

            var response = await _chatService.GetChatMessageContentAsync(
                history, cancellationToken: cts.Token);

            var reply = response.Content ?? "（无回复）";
            _logger.LogInformation("AI 回复：{Reply}", reply);

            return reply;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("AI 调用超时");
            throw new BusinessException($"AI 调用超时（{_options.TimeoutSeconds} 秒）");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI 调用失败");
            throw new BusinessException($"AI 调用失败：{ex.Message}");
        }
    }

    public async Task<bool> TestConnectionAsync(CancellationToken ct = default)
    {
        try
        {
            var reply = await ChatAsync("你好", ct);
            return !string.IsNullOrWhiteSpace(reply);
        }
        catch
        {
            return false;
        }
    }
}
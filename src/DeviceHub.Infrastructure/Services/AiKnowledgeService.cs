using DeviceHub.Core.DTOs;
using DeviceHub.Core.Interfaces;
using DeviceHub.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;

namespace DeviceHub.Infrastructure.Services;

/// <summary>
/// AI 知识库服务
/// 关键词检索 + LLM 生成建议
/// </summary>
public class AiKnowledgeService : IAiKnowledgeService
{
    private readonly IWorkOrderRepository _workOrderRepo;
    private readonly IChatCompletionService _chatService;
    private readonly AiOptions _options;
    private readonly ILogger<AiKnowledgeService> _logger;

    public AiKnowledgeService(
        IWorkOrderRepository workOrderRepo,
        Kernel kernel,
        IOptions<AiOptions> options,
        ILogger<AiKnowledgeService> logger)
    {
        _workOrderRepo = workOrderRepo;
        _options = options.Value;
        _logger = logger;
        _chatService = kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<AiChatResponse> SearchAndSuggestAsync(
        string faultDescription, int topK = 3, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(faultDescription))
            throw new Core.Exceptions.BusinessException("故障描述不能为空");

        var response = new AiChatResponse();

        //提取关键词
        var keywords = ExtractKeywords(faultDescription);
        _logger.LogInformation("提取关键词：{Keywords}", string.Join(", ", keywords));

        // 搜索相似工单
        var similarOrders = await _workOrderRepo.SearchByKeywordsAsync(keywords, topK * 2, ct);

        response.SimilarCases = similarOrders
            .Take(topK)
            .Select(o => new SimilarCaseDto
            {
                Id = o.Id,
                OrderNo = o.OrderNo,
                Title = o.Title,
                Description = o.Description,
                Resolution = o.Resolution,
                DeviceName = o.Device?.Name,
                CreatedAt = o.CreatedAt,
                Similarity = CalculateSimilarity(keywords, o)
            })
            .OrderByDescending(c => c.Similarity)
            .ToList();

        //LLM 生成建议
        response.Suggestion = await GenerateSuggestionAsync(faultDescription, response.SimilarCases, ct);

        return response;
    }

    /// <summary>
    /// 简单关键词提取：去掉标点、按空格/中文分词（简化版）
    /// </summary>
    private static List<string> ExtractKeywords(string text)
    {
        // 去掉标点
        var cleaned = new string(text
            .Where(c => !char.IsPunctuation(c) && !char.IsWhiteSpace(c))
            .ToArray());

        if (cleaned.Length < 2) return new List<string> { cleaned };

        // 2-gram
        var keywords = new List<string>();
        for (int i = 0; i < cleaned.Length - 1; i++)
        {
            keywords.Add(cleaned.Substring(i, 2));
        }

        return keywords.Distinct().Take(10).ToList();
    }

    /// <summary>
    /// 计算相似度：关键词在工单里出现的次数/关键词总数
    /// </summary>
    private static double CalculateSimilarity(List<string> keywords, Core.Entities.WorkOrder order)
    {
        if (keywords.Count == 0) return 0;

        var text = $"{order.Title} {order.Description} {order.Resolution}".ToLower();
        var matched = keywords.Count(k => text.Contains(k.ToLower()));

        return (double)matched / keywords.Count;
    }

    /// <summary>
    /// LLM 生成建议：把相似工单喂给LLM
    /// </summary>
    private async Task<string> GenerateSuggestionAsync(
        string faultDescription,
        List<SimilarCaseDto> similarCases,
        CancellationToken ct)
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("你是一个工业设备故障诊断专家。用户描述了故障，我提供了一些历史相似案例。请给出简洁的处理建议。");
            sb.AppendLine();
            sb.AppendLine($"【用户故障描述】{faultDescription}");
            sb.AppendLine();

            if (similarCases.Count > 0)
            {
                sb.AppendLine("【历史相似案例】");
                foreach (var c in similarCases)
                {
                    sb.AppendLine($"- 工单 {c.OrderNo}：{c.Title}");
                    sb.AppendLine($"  故障：{c.Description}");
                    sb.AppendLine($"  处理：{c.Resolution ?? "（无）"}");
                }
            }
            else
            {
                sb.AppendLine("【历史相似案例】无匹配案例");
            }
            sb.AppendLine();
            sb.AppendLine("请输出 3-5 条处理建议，简洁专业。若无相似案例，请基于工业常识给出建议。");

            var history = new ChatHistory();
            history.AddUserMessage(sb.ToString());

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

            var reply = await _chatService.GetChatMessageContentAsync(history, cancellationToken: cts.Token);
            return reply.Content ?? "（无建议）";
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("AI 生成建议超时");
            return "AI 生成建议超时，请稍后重试。";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI 生成建议失败");
            return $"AI 生成建议失败：{ex.Message}";
        }
    }
}
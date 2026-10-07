namespace DeviceHub.Core.DTOs;

/// <summary>
/// AI对话响应
/// </summary>
public class AiChatResponse
{
    /// <summary>
    /// 相似历史案例
    /// </summary>
    public List<SimilarCaseDto> SimilarCases { get; set; } = new();

    /// <summary>
    /// AI建议
    /// </summary>
    public string Suggestion { get; set; } = string.Empty;

    /// <summary>
    /// 原始AI回复
    /// </summary>
    public string? RawReply { get; set; }
}
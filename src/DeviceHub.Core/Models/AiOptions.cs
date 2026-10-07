namespace DeviceHub.Core.Models;

/// <summary>
/// AI 配置
/// </summary>
public class AiOptions
{
    public const string SectionName = "Ai";

    /// <summary>
    /// LLM API Key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// LLM 服务地址
    /// </summary>
    public string Endpoint { get; set; } = "https://api.deepseek.com/v1";

    /// <summary>
    /// 模型名
    /// </summary>
    public string ModelId { get; set; } = "deepseek-chat";

    /// <summary>
    /// 超时（秒）
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
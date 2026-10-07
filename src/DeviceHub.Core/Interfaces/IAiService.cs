namespace DeviceHub.Core.Interfaces;

/// <summary>
/// AI服务接口
/// </summary>
public interface IAiService
{
    /// <summary>
    /// 发送对话消息，返回AI回复
    /// </summary>
    Task<string> ChatAsync(string message, CancellationToken ct = default);

    /// <summary>
    /// 测试LLM连接
    /// </summary>
    Task<bool> TestConnectionAsync(CancellationToken ct = default);
}
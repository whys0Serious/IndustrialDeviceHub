using DeviceHub.Core.DTOs;

namespace DeviceHub.Client.Services;

/// <summary>
/// AI 客户端服务：调Api的AI接口
/// </summary>
public interface IAiClientService
{
    /// <summary>
    /// 故障诊断（检索相似案例 + 生成建议）
    /// </summary>
    Task<AiChatResponse> DiagnoseAsync(string faultDescription, CancellationToken ct = default);
}
using DeviceHub.Core.DTOs;

namespace DeviceHub.Core.Interfaces;

/// <summary>
/// AI知识库服务
/// 检索相似历史工单+生成建议
/// </summary>
public interface IAiKnowledgeService
{
    /// <summary>
    /// 根据故障描述 检索相似案例+生成建议
    /// </summary>
    Task<AiChatResponse> SearchAndSuggestAsync(
        string faultDescription,
        int topK = 3,
        CancellationToken ct = default);
}
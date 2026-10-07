namespace DeviceHub.Core.DTOs;

/// <summary>
/// 相似历史工单
/// </summary>
public class SimilarCaseDto
{
    public int Id { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Resolution { get; set; }
    public string? DeviceName { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 相似度（0-1）
    /// </summary>
    public double Similarity { get; set; }
}
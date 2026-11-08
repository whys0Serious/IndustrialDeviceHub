namespace DeviceHub.Core.Entities;

/// <summary>
/// 工艺配方实体
/// </summary>
public class Recipe
{
    /// <summary>
    /// 主键
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 配方名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 目标温度（单位：℃，精度 0.1）
    /// </summary>
    public double TargetTemperature { get; set; }

    /// <summary>
    /// 目标压力（单位：kPa）
    /// </summary>
    public double TargetPressure { get; set; }

    /// <summary>
    /// 目标转速（单位：RPM）
    /// </summary>
    public double TargetSpeed { get; set; }

    /// <summary>
    /// 配方创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
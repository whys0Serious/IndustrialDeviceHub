namespace DeviceHub.Core.Entities;

/// <summary>
/// 整机作业执行记录
/// </summary>
public class JobRecord
{
    public int Id { get; set; }

    /// <summary>
    /// 作业流水号
    /// </summary>
    public string JobNo { get; set; } = string.Empty;

    public int DeviceId { get; set; }

    /// <summary>
    /// 执行时的配方快照
    /// </summary>
    public string RecipeName { get; set; } = string.Empty;
    public double TargetTemp { get; set; }
    public double TargetPress { get; set; }
    public double TargetSpeed { get; set; }

    //过程采集汇总
    public double MaxTemp { get; set; }
    public double MinTemp { get; set; }
    public double AvgTemp { get; set; }
    public double MaxPress { get; set; }
    public double AvgPress { get; set; }
    public double MaxSpeed { get; set; }
    public double AvgSpeed { get; set; }

    // 结果判定
    /// <summary>
    /// 工艺结果判定
    /// </summary>
    public bool IsQualified { get; set; }

    /// <summary>
    /// 异常原因（如：温度超差）
    /// </summary>
    public string ExceptionReason { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 状态：Running / Completed / Aborted
    /// </summary>
    public string Status { get; set; } = "Running";
}
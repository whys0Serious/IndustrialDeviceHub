using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Models
{
    /// <summary>
    /// 作业执行结果汇总对象
    /// </summary>
    public record JobResultSummary(

        double MaxTemp,
        double MinTemp, 
        double AvgTemp,

        double MaxPress,
        double AvgPress,

        double MaxSpeed, 
        double AvgSpeed,

        bool IsQualified, 

        string ExceptionReason
    );
}

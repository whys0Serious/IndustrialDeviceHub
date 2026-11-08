using DeviceHub.Core.Entities;
using DeviceHub.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Interfaces
{
    /// <summary>
    /// 整机作业调度与记录服务接口
    /// </summary>
    public interface IJobExecutionService
    {
        /// <summary>
        /// 创建作业记录（启动时调用，状态为 Running）
        /// </summary>
        Task<JobRecord> CreateJobAsync(int deviceId, Recipe recipe);

        /// <summary>
        /// 完成作业并写入结果
        /// </summary>
        Task CompleteJobAsync(int jobId, JobResultSummary result, string status = "Completed");

        /// <summary>
        /// 获取历史作业履历
        /// </summary>
        Task<List<JobRecord>> GetJobHistoryAsync(int deviceId);
    }
}

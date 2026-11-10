using DeviceHub.Core.Common;
using DeviceHub.Core.Entities;
using DeviceHub.Core.Interfaces;
using DeviceHub.Core.Models;
using DeviceHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeviceHub.Infrastructure.Services
{
    /// <summary>
    /// 整机作业调度与记录服务实现
    /// </summary>
    public class JobExecutionService : IJobExecutionService
    {
        private readonly DeviceHubDbContext _db;

        public JobExecutionService(DeviceHubDbContext db) => _db = db;

        public async Task<JobRecord> CreateJobAsync(int deviceId, Recipe recipe)
        {
            var job = new JobRecord
            {
                JobNo = $"JOB-{DateTime.Now:yyyyMMdd-HHmmss}",
                DeviceId = deviceId,
                RecipeName = recipe.Name,
                TargetTemp = recipe.TargetTemperature,
                TargetPress = recipe.TargetPressure,
                TargetSpeed = recipe.TargetSpeed,
                StartTime = DateTime.Now,
                Status = "Running"
            };
            _db.JobRecords.Add(job);
            await _db.SaveChangesAsync();
            return job;
        }

        public async Task CompleteJobAsync(int jobId, JobResultSummary result, string status = "Completed")
        {
            var job = await _db.JobRecords.FindAsync(jobId);
            if (job == null) return;

            job.MaxTemp = result.MaxTemp;
            job.MinTemp = result.MinTemp;
            job.AvgTemp = result.AvgTemp;
            job.MaxPress = result.MaxPress;
            job.AvgPress = result.AvgPress;
            job.MaxSpeed = result.MaxSpeed;
            job.AvgSpeed = result.AvgSpeed;
            job.IsQualified = result.IsQualified;
            job.ExceptionReason = result.ExceptionReason;
            job.EndTime = DateTime.Now;
            job.Status = status;

            await _db.SaveChangesAsync();
        }

        public async Task<PagedResult<JobRecord>> GetJobHistoryAsync(int deviceId, int pageIndex, int pageSize)
        {
            var query = _db.JobRecords.Where(r => r.DeviceId == deviceId);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.StartTime)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<JobRecord>
            {
                Items = items,
                TotalCount = totalCount,
                Page = pageIndex,
                PageSize = pageSize
            };
        }
           
    }
}

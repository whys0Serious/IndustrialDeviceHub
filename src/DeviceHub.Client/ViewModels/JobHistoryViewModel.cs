using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHub.Client.Attributes;
using DeviceHub.Core.Entities;
using DeviceHub.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace DeviceHub.Client.ViewModels
{
    /// <summary>
    /// 作业履历查询与报告导出 ViewModel
    /// </summary>
    [ViewModel(ServiceLifetime.Transient)]
    public partial class JobHistoryViewModel : ObservableObject
    {
        private readonly IJobExecutionService _jobService;

        /// <summary>
        /// 作业历史记录列表
        /// </summary>
        public ObservableCollection<JobRecord> JobRecords { get; } = new();

        /// <summary>
        /// 当前选中的作业记录
        /// </summary>
        [ObservableProperty] private JobRecord? _selectedJob;

        public JobHistoryViewModel(IJobExecutionService jobService) => _jobService = jobService;

        /// <summary>
        /// 加载历史记录
        /// </summary>
        [RelayCommand]
        private async Task LoadAsync()
        {
            var list = await _jobService.GetJobHistoryAsync(1); // 假设设备Id固定为1
            JobRecords.Clear();
            foreach (var r in list) JobRecords.Add(r);
        }

        /// <summary>
        /// 导出/打印工艺流转卡
        /// </summary>
        [RelayCommand]
        private void ExportReport()
        {
            if (SelectedJob == null) return;

            //生成工业流转文本
            var report = new System.Text.StringBuilder();
            report.AppendLine("====================================");
            report.AppendLine("      整机作业工艺流转卡");
            report.AppendLine("====================================");
            report.AppendLine($"作业流水号：{SelectedJob.JobNo}");
            report.AppendLine($"设备编号：{SelectedJob.DeviceId}");
            report.AppendLine($"执行配方：{SelectedJob.RecipeName}");
            report.AppendLine($"开机时间：{SelectedJob.StartTime:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"停机时间：{SelectedJob.EndTime:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine("------------------------------------");

            report.AppendLine("【工艺设定值】");
            report.AppendLine($"温度：{SelectedJob.TargetTemp} ℃ | 压力：{SelectedJob.TargetPress} kPa | 转速：{SelectedJob.TargetSpeed} RPM");
            report.AppendLine("------------------------------------");

            report.AppendLine("【设备实际运行反馈】");
            report.AppendLine($"温度 -> 均温：{SelectedJob.AvgTemp:F1} ℃ | 最高：{SelectedJob.MaxTemp:F1} ℃");
            report.AppendLine($"压力 -> 均值：{SelectedJob.AvgPress} kPa | 最高：{SelectedJob.MaxPress} kPa");
            report.AppendLine($"转速 -> 均速：{SelectedJob.AvgSpeed:F0} RPM | 最高：{SelectedJob.MaxSpeed} RPM");
            report.AppendLine("------------------------------------");

            report.AppendLine($"质检结论：{(SelectedJob.IsQualified ? "✅ 合格 (PASS)" : "❌ 不合格 (FAIL)")}");
            if (!SelectedJob.IsQualified)
            {
                report.AppendLine($"异常原因：{SelectedJob.ExceptionReason}");
            }
            report.AppendLine("====================================");
            report.AppendLine("操作员签字：__________   质检员签字：__________");

            // 2. 保存至桌面
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = System.IO.Path.Combine(desktopPath, $"{SelectedJob.JobNo}_工艺.txt");
            System.IO.File.WriteAllText(filePath, report.ToString());

            System.Windows.MessageBox.Show($"已生成至桌面：\n{filePath}", "导出成功",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
    }
}

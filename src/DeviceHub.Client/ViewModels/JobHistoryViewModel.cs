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
using System.Windows.Media;

namespace DeviceHub.Client.ViewModels
{
    /// <summary>
    /// 作业履历查询与报告导出 ViewModel
    /// </summary>
    [ViewModel(ServiceLifetime.Transient)]
    public partial class JobHistoryViewModel : ObservableObject
    {
        private readonly IJobExecutionService _jobService;

        // 并发防抖锁（防止用户疯狂点击刷新或下一页导致 DbContext 崩溃）
        private readonly SemaphoreSlim _loadLock = new(1, 1);
        /// <summary>
        /// 作业历史记录列表
        /// </summary>
        public ObservableCollection<JobRecord> JobRecords { get; } = new();

        // 分页控制属性
        [ObservableProperty] private int _currentPageIndex = 1;
        [ObservableProperty] private int _pageSize = 10;
        [ObservableProperty] private int _totalPages;
        [ObservableProperty] private int _totalCount;

        // 状态控制
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private string? _errorMessage;
        /// <summary>
        /// 当前选中的作业记录
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ExportReportCommand))]
        private JobRecord? _selectedJob;

        /// 是否选中了记录（用于控制导出按钮状态）
        public bool IsRecordSelected => SelectedJob != null;
        public bool HasData => JobRecords.Count > 0;
        public JobHistoryViewModel(IJobExecutionService jobService) => _jobService = jobService;

        public async Task InitializeAsync()
        {
            await LoadPageAsync();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            CurrentPageIndex = 1;
            await LoadPageAsync();
        }

        [RelayCommand]
        private async Task PageUpdatedAsync()
        {
            await LoadPageAsync();
        }

        /// <summary>
        /// 翻页或刷新时加载数据
        /// </summary>
        private async Task LoadPageAsync()
        {
            // 加锁防抖
            if (!await _loadLock.WaitAsync(0))
            {
                System.Diagnostics.Debug.WriteLine("[JobHistory] 上一次查询未完成，跳过本次");
                return;
            }

            IsLoading = true;
            ErrorMessage = null;

            try
            {
                
                var result = await _jobService.GetJobHistoryAsync(1, CurrentPageIndex, PageSize);

                JobRecords.Clear();
                foreach (var r in result.Items) JobRecords.Add(r);

                TotalCount = result.TotalCount;
                TotalPages = result.TotalPages;
                CurrentPageIndex = result.Page;

                OnPropertyChanged(nameof(HasData));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"加载失败：{ex.Message}";
            }
            finally
            {
                IsLoading = false;
                _loadLock.Release();
            }
        }

        /// <summary>
        /// 导出/打印工艺流转卡
        /// </summary>
        [RelayCommand(CanExecute = nameof(IsRecordSelected))]
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

            // 保存至桌面
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = System.IO.Path.Combine(desktopPath, $"{SelectedJob.JobNo}_工艺.txt");
            System.IO.File.WriteAllText(filePath, report.ToString());

            System.Windows.MessageBox.Show($"已生成至桌面：\n{filePath}", "导出成功",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
    }
}

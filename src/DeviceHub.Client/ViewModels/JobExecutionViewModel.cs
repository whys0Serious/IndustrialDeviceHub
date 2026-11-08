using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHub.Client.Attributes;
using DeviceHub.Core.Entities;
using DeviceHub.Core.Interfaces;
using DeviceHub.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Threading;

namespace DeviceHub.Client.ViewModels
{
    /// <summary>
    /// 控制“参数选择 → 下发 → 采集 → 判定 → 存库”
    /// </summary>
    [ViewModel(ServiceLifetime.Transient)]
    public partial class JobExecutionViewModel : ObservableObject, IDisposable
    {
        private readonly IDeviceCommunication _communication;
        private readonly IRecipeService _recipeService;
        private readonly IJobExecutionService _jobService;

        private DispatcherTimer? _timer;
        private int _currentJobId;
        private DateTime _startTime;

        // 过程数据缓存
        private readonly List<double> _temps = new();
        private readonly List<double> _presses = new();
        private readonly List<double> _speeds = new();

        //设定固定运行时长（秒）
        private const int JobDurationSeconds = 15;

        public JobExecutionViewModel(IDeviceCommunication comm, IRecipeService recipeService, IJobExecutionService jobService)
        {
            _communication = comm;
            _recipeService = recipeService;
            _jobService = jobService;
        }

        
        /// <summary>
        /// 当前步骤（0:配置 1:运行 2:结果）
        /// </summary>
        [ObservableProperty] private int _currentStep = 0;

        /// <summary>
        /// 状态提示文本
        /// </summary>
        [ObservableProperty] private string _statusMessage = "请选择配方";

        /// <summary>
        /// 进度百分比（0-100）
        /// </summary>
        [ObservableProperty] private double _progress = 0;

        /// <summary>
        /// 配方下拉列表
        /// </summary>
        public ObservableCollection<Recipe> Recipes { get; } = new();

        /// <summary>
        /// 当前选中的配方
        /// </summary>
        [ObservableProperty] private Recipe? _selectedRecipe;

        /// <summary>
        /// 实时温度显示
        /// </summary>
        [ObservableProperty] private double _currentTemp;

        /// <summary>
        /// 实时压力显示
        /// </summary>
        [ObservableProperty] private double _currentPress;

        /// <summary>
        /// 实时转速显示
        /// </summary>
        [ObservableProperty] private double _currentSpeed;

        /// <summary>
        /// 最终结果详细描述
        /// </summary>
        [ObservableProperty] private string _resultSummary = string.Empty;

        /// <summary>
        /// 综合判定是否合格
        /// </summary>
        [ObservableProperty] private bool _isQualified;

        [RelayCommand]
        private async Task LoadRecipesAsync()
        {
            var list = await _recipeService.GetAllAsync();
            Recipes.Clear();
            foreach (var r in list) Recipes.Add(r);
            if (Recipes.Count > 0) SelectedRecipe = Recipes.First();
        }

        /// <summary>
        /// 启动作业
        /// </summary>
        [RelayCommand]
        private async Task StartJobAsync()
        {
            if (SelectedRecipe == null) { StatusMessage = "请先选择配方"; return; }

            try
            {
                CurrentStep = 1;
                Progress = 0;
                _temps.Clear(); _presses.Clear(); _speeds.Clear();
                StatusMessage = "正在下发工艺参数...";

                //数值转换并下发参数（温度放大10倍，压力、转速原样）
                ushort rawTemp = (ushort)(SelectedRecipe.TargetTemperature * 10);
                ushort rawPress = (ushort)SelectedRecipe.TargetPressure;
                ushort rawSpeed = (ushort)SelectedRecipe.TargetSpeed;

                await _communication.WriteSingleRegisterAsync(1, 10, rawTemp);
                await _communication.WriteSingleRegisterAsync(1, 11, rawPress);
                await _communication.WriteSingleRegisterAsync(1, 12, rawSpeed);

                //创建作业记录（状态：Running）
                var job = await _jobService.CreateJobAsync(1, SelectedRecipe);
                _currentJobId = job.Id;
                _startTime = DateTime.Now;

                //启动定时采集
                StatusMessage = "工艺参数已生效，设备运行中...";
                _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                _timer.Tick += async (_, _) => await PollDataAsync();
                _timer.Start();
            }
            catch (Exception ex)
            {
                StatusMessage = $"启动失败：{ex.Message}";
                CurrentStep = 0;
            }
        }

        /// <summary>
        /// 轮询采集数据
        /// </summary>
        private async Task PollDataAsync()
        {
            try
            {
                var registers = await _communication.ReadHoldingRegistersAsync(1, 0, 3);
                if (registers.Length < 3 || SelectedRecipe == null) return;

                // 读回并反向转换
                CurrentTemp = registers[0] / 10.0;
                CurrentPress = registers[1];
                CurrentSpeed = registers[2];

                _temps.Add(CurrentTemp);
                _presses.Add(CurrentPress);
                _speeds.Add(CurrentSpeed);

                // 更新进度
                var elapsed = (DateTime.Now - _startTime).TotalSeconds;
                Progress = Math.Min(100, (elapsed / JobDurationSeconds) * 100);

                // 到达时长则结束
                if (elapsed >= JobDurationSeconds)
                {
                    _timer?.Stop();
                    await CompleteJobAsync();
                }
            }
            catch (Exception ex)
            {
                _timer?.Stop();
                StatusMessage = $"作业中断：{ex.Message}";
                await _jobService.CompleteJobAsync(_currentJobId, new JobResultSummary(0, 0, 0, 0, 0, 0, 0, false, ex.Message), "Aborted");
                CurrentStep = 2;
            }
        }

        /// <summary>
        /// 完成作业，计算并存储结果
        /// </summary>
        private async Task CompleteJobAsync()
        {
            CurrentStep = 2;
            StatusMessage = "设备运行结束，正在生成履历...";

            if (_temps.Count == 0) return;

            //计算汇总数据
            double maxTemp = _temps.Max();
            double minTemp = _temps.Min();
            double avgTemp = _temps.Average();
            double maxPress = _presses.Max();
            double avgPress = _presses.Average();
            double maxSpeed = _speeds.Max();
            double avgSpeed = _speeds.Average();

            //多维度工艺判定
            var exceptions = new List<string>();
            if (Math.Abs(avgTemp - SelectedRecipe!.TargetTemperature) > 2.0) exceptions.Add("温度超差");
            if (Math.Abs(maxPress - SelectedRecipe.TargetPressure) > 50) exceptions.Add("压力超差");
            if (Math.Abs(avgSpeed - SelectedRecipe.TargetSpeed) > 30) exceptions.Add("转速超差");

            IsQualified = exceptions.Count == 0;
            string exceptionStr = string.Join("；", exceptions);

            //更新 UI 结果展示
            ResultSummary = $"【温度】目标：{SelectedRecipe.TargetTemperature}℃ | 均温：{avgTemp:F1}℃\n" +
                            $"【压力】目标：{SelectedRecipe.TargetPressure}kPa | 极值：{maxPress}kPa\n" +
                            $"【转速】目标：{SelectedRecipe.TargetSpeed}RPM | 均速：{avgSpeed:F0}RPM\n\n" +
                            $"最终判定：{(IsQualified ? "✅ 合格" : $"❌ 不合格\n异常原因：{exceptionStr}")}";

            //保存结果至数据库
            var summary = new JobResultSummary(
                maxTemp, minTemp, avgTemp,
                maxPress, avgPress,
                maxSpeed, avgSpeed,
                IsQualified, exceptionStr
            );

            await _jobService.CompleteJobAsync(_currentJobId, summary);
            StatusMessage = "作业记录已归档";
        }

        ///<summary>
        ///重置状态，准备下一次作业
        ///</summary>
        [RelayCommand]
        private void Reset()
        {
            _timer?.Stop();
            CurrentStep = 0;
            StatusMessage = "请选择配方";
            Progress = 0;
        }

        public void Dispose() => _timer?.Stop();
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using DeviceHub.Client.Attributes;
using DeviceHub.Core.DTOs;
using DeviceHub.Core.Interfaces;
using DeviceHub.Infrastructure.Services;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Threading;

namespace DeviceHub.Client.ViewModels
{
    [ViewModel(ServiceLifetime.Transient)]
    public partial class RealtimeMonitorViewModel : ObservableObject, IDisposable
    {
        private readonly IRealtimeDataService _realtimeService;

        private readonly IDeviceService _deviceService;
        private readonly DispatcherTimer _timer;

        /// <summary>
        /// 保留的数据点数量（60 秒 × 1Hz）
        /// </summary>
        private const int MaxPoints = 60;


        //当前选中的设备
        [ObservableProperty]
        private DeviceDto? _selectedDevice;


        [ObservableProperty] private string? _errorMessage;

        public ObservableCollection<DeviceDto> DeviceOptions { get; } = new();

        //初始化加载启用监控的设备
        public async Task InitializeAsync()
        {
            await LoadDevicesAsync();
            _timer.Start();
        }
        public RealtimeMonitorViewModel(IRealtimeDataService realtimeService, IDeviceService deviceService)
        {
            _realtimeService = realtimeService;
            _deviceService = deviceService;
            //压力曲线
            PressureValues = new ObservableCollection<DateTimePoint>();
            PressureSeries = new ISeries[]
            {
            new LineSeries<DateTimePoint>
                {
                    Name = "压力 (kPa)",
                    Values = PressureValues,
                    GeometrySize = 0,
                    Stroke = new SolidColorPaint(SKColors.DodgerBlue, 2),
                    Fill = new SolidColorPaint(SKColors.DodgerBlue.WithAlpha(30))
                }
            };
            PressureXAxes = new Axis[]
            {
                new Axis
                {
                    Labeler = value =>
                    {
                        if (value < DateTime.MinValue.Ticks || value > DateTime.MaxValue.Ticks)
                            return string.Empty;
                        return new DateTime((long)value).ToString("HH:mm:ss");
                    },
                    UnitWidth = TimeSpan.FromSeconds(1).Ticks,
                    MinStep = TimeSpan.FromSeconds(1).Ticks,
                    TextSize = 11
                }
            };
            PressureYAxes = new Axis[]
            {
                new Axis
                {
                    Name = "kPa",
                    TextSize = 11,
                    MinLimit = 0
                }
            };

            //转速曲线
            SpeedValues = new ObservableCollection<DateTimePoint>();
            SpeedSeries = new ISeries[]
            {
            new LineSeries<DateTimePoint>
                {
                    Name = "转速 (RPM)",
                    Values = SpeedValues,
                    GeometrySize = 0,
                    Stroke = new SolidColorPaint(SKColors.OrangeRed, 2),
                    Fill = new SolidColorPaint(SKColors.OrangeRed.WithAlpha(30))
                }
            };
            SpeedXAxes = new Axis[]
            {
            new Axis
                {
                    Labeler = value =>
                    {
                        if (value < DateTime.MinValue.Ticks || value > DateTime.MaxValue.Ticks)
                            return string.Empty;
                        return new DateTime((long)value).ToString("HH:mm:ss");
                    },
                    UnitWidth = TimeSpan.FromSeconds(1).Ticks,
                    MinStep = TimeSpan.FromSeconds(1).Ticks,
                    TextSize = 11
                }
            };
            SpeedYAxes = new Axis[]
            {
            new Axis
                {
                    Name = "RPM",
                    TextSize = 11,
                    MinLimit = 0
                }
            };
            // 定时刷新（每秒）
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (_, _) => Refresh();
        }

        /// <summary>
        /// 压力曲线
        /// </summary>

        public ObservableCollection<DateTimePoint> PressureValues { get; }

        public ISeries[] PressureSeries { get; }

        public Axis[] PressureXAxes { get; }

        public Axis[] PressureYAxes { get; }

        /// <summary>
        /// 转速曲线
        /// </summary>

        public ObservableCollection<DateTimePoint> SpeedValues { get; }

        public ISeries[] SpeedSeries { get; }

        public Axis[] SpeedXAxes { get; }

        public Axis[] SpeedYAxes { get; }


        /// <summary>
        /// 温度
        /// </summary>

        [ObservableProperty]
        private double _temperature;

        [ObservableProperty]
        private double _temperatureMax = 100;   //温度上限（用于进度百分比）

        /// <summary>
        /// 当前值
        /// </summary>

        [ObservableProperty]
        private double _currentPressure;

        [ObservableProperty]
        private int _currentSpeed;

        [ObservableProperty]
        private DateTime? _lastUpdateTime;

        /// <summary>
        /// 连接状态
        /// </summary>

        [ObservableProperty]
        private bool _isConnected;

        public string ConnectionText => IsConnected ? "已连接" : "未连接";

        partial void OnIsConnectedChanged(bool value)
        {
            OnPropertyChanged(nameof(ConnectionText));
        }

        /// <summary>
        /// 生命周期
        /// </summary>

        public void Initialize()
        {
            _timer.Start();
        }

        public void Dispose()
        {
            _timer.Stop();
        }

        //设备切换时清空曲线
        partial void OnSelectedDeviceChanged(DeviceDto? value)
        {
            ClearCharts();
        }


          private async Task LoadDevicesAsync()
    {
        try
        {
            var result = await _deviceService.GetPagedAsync(new DeviceQueryDto
            {
                Page = 1,
                PageSize = 200,
                // 可选：只显示启用监控的
            });

            DeviceOptions.Clear();
            foreach (var d in result.Items)
            {
                // 只加启用监控的设备
                if (d.EnableMonitoring)
                    DeviceOptions.Add(d);
            }

            SelectedDevice = DeviceOptions.FirstOrDefault();
        }
        catch(Exception ex)
        {
            ErrorMessage = $"加载设备失败：{ex.Message}";
        }
    }

        private void ClearCharts()
        {
            PressureValues.Clear();
            SpeedValues.Clear();
            Temperature = 0;
            CurrentPressure = 0;
            CurrentSpeed = 0;
            LastUpdateTime = null;
        }

        /// <summary>
        ///定时刷新
        /// </summary>

        private void Refresh()
        {
            if (SelectedDevice == null)
            {
                IsConnected = false;
                return;
            }
           
            var data = _realtimeService.GetLatest(SelectedDevice.Id);
            IsConnected = _realtimeService.IsConnected;

            if (data == null) return;

            Temperature = data.Temperature;
            CurrentPressure = data.Pressure;
            CurrentSpeed = data.Speed;

            PressureValues.Add(new DateTimePoint(data.Timestamp, data.Pressure));
            TrimOldPoints(PressureValues);


            SpeedValues.Add(new DateTimePoint(data.Timestamp, data.Speed));
            TrimOldPoints(SpeedValues);

            LastUpdateTime = data.Timestamp;
        }

        /// <summary>
        /// 保留最近 MaxPoints 个数据点
        /// </summary>
        private static void TrimOldPoints(ObservableCollection<DateTimePoint> values)
        {
            while (values.Count > MaxPoints)
            {
                values.RemoveAt(0);
            }
        }
    }
}

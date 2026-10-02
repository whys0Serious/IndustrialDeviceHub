using DeviceHub.Core.Enums;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,
    ResourceDictionaryLocation.SourceAssembly
)]
namespace DeviceHub.Controls.Controls;

/// <summary>
/// 设备状态指示灯：纯圆形灯，鼠标悬浮显示状态文本
/// </summary>
public class DeviceStatusIndicator : Control
{
    static DeviceStatusIndicator()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(DeviceStatusIndicator),
            new FrameworkPropertyMetadata(typeof(DeviceStatusIndicator)));
    }

    public DeviceStatusIndicator()
    {
        UpdateStatusText();
        UpdateStatusBrush();
    }

    /// <summary>
    /// 状态
    /// </summary>

    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(
            nameof(Status),
            typeof(DeviceStatus),
            typeof(DeviceStatusIndicator),
            new PropertyMetadata(DeviceStatus.Stopped, OnStatusChanged));

    /// <summary>
    /// 设备状态
    /// </summary>
    public DeviceStatus Status
    {
        get => (DeviceStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (DeviceStatusIndicator)d;
        control.UpdateStatusText();
        control.UpdateStatusBrush();
    }

    /// <summary>
    /// 状态文本（只读）
    /// </summary>

    private static readonly DependencyPropertyKey StatusTextPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(StatusText),
            typeof(string),
            typeof(DeviceStatusIndicator),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty StatusTextProperty =
        StatusTextPropertyKey.DependencyProperty;

    /// <summary>
    /// 状态显示文本
    /// </summary>
    public string StatusText
    {
        get => (string)GetValue(StatusTextProperty);
        private set => SetValue(StatusTextPropertyKey, value);
    }

    /// <summary>
    /// 状态颜色（只读）
    /// </summary>

    private static readonly DependencyPropertyKey StatusBrushPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(StatusBrush),
            typeof(Brush),
            typeof(DeviceStatusIndicator),
            new PropertyMetadata(Brushes.Gray));

    public static readonly DependencyProperty StatusBrushProperty =
        StatusBrushPropertyKey.DependencyProperty;

    /// <summary>
    /// 状态颜色
    /// </summary>
    public Brush StatusBrush
    {
        get => (Brush)GetValue(StatusBrushProperty);
        private set => SetValue(StatusBrushPropertyKey, value);
    }

    /// <summary>
    /// 圆点大小 
    /// </summary>

    public static readonly DependencyProperty DotSizeProperty =
        DependencyProperty.Register(
            nameof(DotSize),
            typeof(double),
            typeof(DeviceStatusIndicator),
            new PropertyMetadata(12.0));

    /// <summary>
    /// 圆点大小（像素），默认 12
    /// </summary>
    public double DotSize
    {
        get => (double)GetValue(DotSizeProperty);
        set => SetValue(DotSizeProperty, value);
    }

    private void UpdateStatusText()
    {
        StatusText = Status switch
        {
            DeviceStatus.Running => "运行中",
            DeviceStatus.Alarm => "报警",
            DeviceStatus.Stopped => "已停止",
            DeviceStatus.Maintenance => "维护中",
            _ => "未知"
        };
    }

    private void UpdateStatusBrush()
    {
        var key = Status switch
        {
            DeviceStatus.Running => "SuccessBrush",
            DeviceStatus.Alarm => "DangerBrush",
            DeviceStatus.Stopped => "TextDisabledBrush",
            DeviceStatus.Maintenance => "WarningBrush",
            _ => "TextDisabledBrush"
        };

        if (TryFindResource(key) is Brush brush)
        {
            StatusBrush = brush;
        }
        else
        {
            StatusBrush = Status switch
            {
                DeviceStatus.Running => new SolidColorBrush(Color.FromRgb(0x27, 0xAE, 0x60)),
                DeviceStatus.Alarm => new SolidColorBrush(Color.FromRgb(0xE7, 0x4C, 0x3C)),
                DeviceStatus.Stopped => new SolidColorBrush(Color.FromRgb(0x95, 0xA5, 0xA6)),
                DeviceStatus.Maintenance => new SolidColorBrush(Color.FromRgb(0xFA, 0xAD, 0x14)),
                _ => Brushes.Gray
            };
        }
    }
}
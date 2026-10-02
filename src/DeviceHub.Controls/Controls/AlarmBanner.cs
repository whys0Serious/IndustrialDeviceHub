using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DeviceHub.Controls.Controls;

/// <summary>
/// 报警提示条：顶部滑入，显示报警数量和最新消息，可关闭。
/// </summary>
public class AlarmBanner : Control
{
    static AlarmBanner()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(AlarmBanner),
            new FrameworkPropertyMetadata(typeof(AlarmBanner)));
    }

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(AlarmBanner),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// 报警消息
    /// </summary>
    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly DependencyProperty CountProperty =
        DependencyProperty.Register(
            nameof(Count),
            typeof(int),
            typeof(AlarmBanner),
            new PropertyMetadata(0));

    /// <summary>
    /// 未确认报警数量
    /// </summary>
    public int Count
    {
        get => (int)GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }

    /// <summary>
    /// IsOpen（控制滑入/滑出）
    /// </summary>

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(
            nameof(IsOpen),
            typeof(bool),
            typeof(AlarmBanner),
            new PropertyMetadata(false));

    /// <summary>
    /// 是否显示（true = 滑入，false = 滑出）
    /// </summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var banner = (AlarmBanner)d;
        banner.SetCurrentValue(VisibilityProperty,
            banner.IsOpen ? Visibility.Visible : Visibility.Collapsed);
    }


    public static readonly DependencyProperty ViewCommandProperty =
        DependencyProperty.Register(
            nameof(ViewCommand),
            typeof(ICommand),
            typeof(AlarmBanner),
            new PropertyMetadata(null));

    /// <summary>
    /// 查看按钮命令
    /// </summary>
    public ICommand? ViewCommand
    {
        get => (ICommand?)GetValue(ViewCommandProperty);
        set => SetValue(ViewCommandProperty, value);
    }


    public static readonly DependencyProperty CloseCommandProperty =
        DependencyProperty.Register(
            nameof(CloseCommand),
            typeof(ICommand),
            typeof(AlarmBanner),
            new PropertyMetadata(null));

    /// <summary>
    /// 关闭按钮命令
    /// </summary>
    public ICommand? CloseCommand
    {
        get => (ICommand?)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    private void UpdateVisualState()
    {
        //用VisualStateManager 触发动画（在模板里定义状态）
        var state = IsOpen ? "Open" : "Closed";
        VisualStateManager.GoToState(this, state, true);
    }
}
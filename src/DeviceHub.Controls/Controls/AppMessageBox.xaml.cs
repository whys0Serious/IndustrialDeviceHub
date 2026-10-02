using System.Windows;
using System.Windows.Controls;

namespace DeviceHub.Controls.Controls;

public partial class AppMessageBox : CustomWindow
{
    private AppMessageBox()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 显示一个消息框（只有确定按钮）
    /// </summary>
    public static void ShowInfo(string message, string title = "提示")
        => Show(message, title, AppMessageBoxType.Info, showCancel: false);

    public static void ShowSuccess(string message, string title = "成功")
        => Show(message, title, AppMessageBoxType.Success, showCancel: false);

    public static void ShowWarning(string message, string title = "警告")
        => Show(message, title, AppMessageBoxType.Warning, showCancel: false);

    public static void ShowError(string message, string title = "错误")
        => Show(message, title, AppMessageBoxType.Error, showCancel: false);

    /// <summary>
    /// 确认框（确定+取消）
    /// 返回用户是否点了确定
    /// </summary>
    public static bool Confirm(string message, string title = "确认")
    {
        var result = Show(message, title, AppMessageBoxType.Question, showCancel: true);
        return result == true;
    }

    private static bool? Show(string message, string title,
        AppMessageBoxType type, bool showCancel)
    {
        var box = new AppMessageBox
        {
            Title = title,
            Owner = Application.Current.MainWindow
        };

        box.MessageText.Text = message;
        box.ApplyIcon(type);
        box.SecondaryButton.Visibility = showCancel ? Visibility.Visible : Visibility.Collapsed;

        // 只有一个按钮时，确定按钮文本用知道了更友好
        box.PrimaryButton.Content = showCancel ? "确定" : "知道了";

        return box.ShowDialog();
    }

    /// <summary>
    /// 图标
    /// </summary>
    /// <param name="type"></param>
    private void ApplyIcon(AppMessageBoxType type)
    {
        (string glyph, string brushKey) = type switch
        {
            AppMessageBoxType.Info => ("\uE946", "PrimaryBrush"),
            AppMessageBoxType.Success => ("\uE73E", "SuccessBrush"),
            AppMessageBoxType.Warning => ("\uE7BA", "WarningBrush"),
            AppMessageBoxType.Error => ("\uEA39", "DangerBrush"),
            AppMessageBoxType.Question => ("\uE897", "PrimaryBrush"),
            _ => ("\uE946", "PrimaryBrush")
        };

        IconText.Text = glyph;

        //用SetResourceReference主题切换时自动更新
        IconText.SetResourceReference(TextBlock.ForegroundProperty, brushKey);
    }

    private void PrimaryButton_Click(object sender, RoutedEventArgs e) => DialogResult = true;

    private void SecondaryButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
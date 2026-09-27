using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

namespace DeviceHub.Client.Services
{
    /// <summary>
    /// 弹出框服务
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// 显示一个通用对话框
        /// (泛型ViewModel+View）
        /// </summary>
        bool? ShowDialog<TViewModel, TView>(Action<TViewModel>? initialize = null)
            where TViewModel : ObservableObject
            where TView : Window, new();

        /// <summary>
        /// 确认框（是/否）
        /// </summary>
        bool Confirm(string message, string title = "确认");

        /// <summary>
        /// 信息提示
        /// </summary>
        void ShowInfo(string message, string title = "提示");

        /// <summary>
        /// 成功提示
        /// </summary>
        void ShowSuccess(string message, string title = "成功");

        /// <summary>
        /// 警告提示
        /// </summary>
        void ShowWarning(string message, string title = "警告");

        /// <summary>
        /// 错误提示
        /// </summary>
        void ShowError(string message, string title = "错误");
    }
}

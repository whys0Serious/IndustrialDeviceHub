using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Client.Services
{
    /// <summary>
    /// 弹出框服务
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// 显示一个对话框（传入ViewModel返回是否成功）
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel类型</typeparam>
        /// <typeparam name="TView">View类型（Window）</typeparam>
        /// <returns>true 表示用户确认；false 表示取消或关闭</returns>
        bool? ShowDialog<TViewModel, TView>(Action<TViewModel>? initialize = null)
            where TViewModel : ObservableObject
            where TView : System.Windows.Window, new();

        /// <summary>
        /// 显示确认框（是/否）
        /// </summary>
        bool Confirm(string message, string title = "确认");

        /// <summary>
        /// 显示消息框
        /// </summary>
        void ShowMessage(string message, string title = "提示");

        /// <summary>
        /// 显示错误框
        /// </summary>
        void ShowError(string message, string title = "错误");
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using DeviceHub.Client.ViewModels;
using DeviceHub.Client.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace DeviceHub.Client.Services
{
    public class DialogService : IDialogService
    {
        private readonly IServiceProvider _services;

        public DialogService(IServiceProvider services)
        {
            _services = services;
        }

        public bool? ShowDialog<TViewModel, TView>(Action<TViewModel>? initialize = null)
            where TViewModel : ObservableObject
            where TView : Window, new()
        {
            // 从DI拿 ViewModel
            var vm = _services.GetRequiredService<TViewModel>();

            //执行初始化回调（如加载数据、设置初始值）
            initialize?.Invoke(vm);

            //创建View并设置DataContext
            var view = new TView
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };

            // 4. 显示模态对话框
            return view.ShowDialog();
        }

        public bool Confirm(string message, string title = "确认")
        {
            var result = MessageBox.Show(
                Application.Current.MainWindow,
                message, title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        public void ShowMessage(string message, string title = "提示")
        {
            MessageBox.Show(
                Application.Current.MainWindow,
                message, title,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        public void ShowError(string message, string title = "错误")
        {
            MessageBox.Show(
                Application.Current.MainWindow,
                message, title,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}

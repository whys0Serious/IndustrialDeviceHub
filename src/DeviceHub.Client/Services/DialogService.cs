using CommunityToolkit.Mvvm.ComponentModel;
using DeviceHub.Controls.Controls;
using HandyControl.Data;
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
            var vm = _services.GetRequiredService<TViewModel>();
            initialize?.Invoke(vm);

            var view = new TView
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };

            return view.ShowDialog();
        }

        public bool Confirm(string message, string title = "确认")
            => AppMessageBox.Confirm(message, title);
        public void ShowInfo(string message, string title = "提示")
         => AppMessageBox.ShowError(message, title);
        public void ShowSuccess(string message, string title = "成功")
            => AppMessageBox.ShowInfo(message, title);

        public void ShowWarning(string message, string title = "警告")
            => AppMessageBox.ShowWarning(message, title);

        public void ShowError(string message, string title = "错误")
            => AppMessageBox.ShowError(message, title);


    }
}

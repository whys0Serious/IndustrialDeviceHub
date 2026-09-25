using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHub.Client.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Client.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly INavigationService _navigation;

        public MainWindowViewModel(INavigationService navigation)
        {
            _navigation = navigation;

            // 把NavigationService的属性变化转发到CurrentViewModel
            _navigation.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(INavigationService.CurrentViewModel))
                    OnPropertyChanged(nameof(CurrentViewModel));
            };
        }

        /// <summary>
        /// 当前显示的ViewMode（ContentControl 绑定它）
        /// </summary>
        public ObservableObject? CurrentViewModel => _navigation.CurrentViewModel;

        /// <summary>
        /// 导航到设备管理
        /// </summary>
        [RelayCommand]
        private void NavigateToDeviceList() => _navigation.NavigateTo<DeviceListViewModel>();

        /// <summary>
        /// 启动时默认导航到设备管理
        /// </summary>
        public void Initialize() => _navigation.NavigateTo<DeviceListViewModel>();
    }
}

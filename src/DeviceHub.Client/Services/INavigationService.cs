using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace DeviceHub.Client.Services
{
    /// <summary>
    /// 导航服务管理当前显示的ViewModel
    /// 由MainWindowViewModel消费
    /// </summary>
    public interface INavigationService : INotifyPropertyChanged
    {
        /// <summary>
        /// 当前显示的ViewModel
        /// </summary>
        ObservableObject? CurrentViewModel { get; }

        /// <summary>
        /// 按ViewModel类型导航（泛型版，类型安全）
        /// </summary>
        void NavigateTo<TViewModel>() where TViewModel : ObservableObject;

        /// <summary>
        /// 按ViewModel类型导航
        /// </summary>
        void NavigateTo(Type viewModelType);
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace DeviceHub.Client.Services
{
    public class NavigationService : ObservableObject, INavigationService
    {
        private readonly IServiceProvider _services;

        public NavigationService(IServiceProvider services)
        {
            _services = services;
        }

        private ObservableObject? _currentViewModel;
        public ObservableObject? CurrentViewModel
        {
            get => _currentViewModel;
            private set => SetProperty(ref _currentViewModel, value);
        }

        public void NavigateTo<TViewModel>() where TViewModel : ObservableObject
        => NavigateTo(typeof(TViewModel));

        public void NavigateTo(Type viewModelType)
        {
            if (!typeof(ObservableObject).IsAssignableFrom(viewModelType))
                throw new ArgumentException(
                    $"{viewModelType.FullName} 必须继承 ObservableObject",
                    nameof(viewModelType));

            var vm = (ObservableObject)_services.GetRequiredService(viewModelType);
            CurrentViewModel = vm;
        }
    }
}

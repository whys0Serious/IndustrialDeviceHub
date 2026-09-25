using CommunityToolkit.Mvvm.ComponentModel;
using DeviceHub.Client.Attributes;
using DeviceHub.Core.DTOs;
using DeviceHub.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace DeviceHub.Client.ViewModels
{
    [ViewModel(ServiceLifetime.Transient)]
    public partial class DeviceListViewModel : ObservableObject
    {
        private readonly IDeviceService _deviceService;

        public DeviceListViewModel(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        public ObservableCollection<DeviceDto> Devices { get; } = new();

        public async Task LoadAsync()
        {
            var result = await _deviceService.GetPagedAsync(new DeviceQueryDto
            {
                PageSize = 100
            });

            Devices.Clear();
            foreach (var d in result.Items)
                Devices.Add(d);
        }
    }

}

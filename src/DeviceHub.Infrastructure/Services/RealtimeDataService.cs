using DeviceHub.Core.DTOs;
using DeviceHub.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Infrastructure.Services
{
    public class RealtimeDataService : IRealtimeDataService
    {
        private readonly IRealtimeDataCache _cache;
        private readonly IDeviceCommunication _communication;

        public RealtimeDataService(
            IRealtimeDataCache cache,
            IDeviceCommunication communication)
        {
            _cache = cache;
            _communication = communication;
        }

        public RealtimeDataDto? GetLatest(int deviceId) => _cache.Get(deviceId);

        public IReadOnlyCollection<RealtimeDataDto> GetAllLatest() => _cache.GetAll();

        public bool IsConnected => _communication.IsConnected;
    }
}

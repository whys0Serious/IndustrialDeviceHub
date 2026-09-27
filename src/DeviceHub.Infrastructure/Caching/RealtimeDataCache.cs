using DeviceHub.Core.DTOs;
using DeviceHub.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Infrastructure.Caching
{
    /// <summary>
    /// 实时数据缓存（线程安全，单例）
    /// </summary>
    public class RealtimeDataCache : IRealtimeDataCache
    {
        private readonly ConcurrentDictionary<int, RealtimeDataDto> _cache = new();

        public void Update(int deviceId, RealtimeDataDto data)
        {
            _cache[deviceId] = data;
        }

        public RealtimeDataDto? Get(int deviceId)
        {
            return _cache.TryGetValue(deviceId, out var data) ? data : null;
        }

        public IReadOnlyCollection<RealtimeDataDto> GetAll()
        {
            return _cache.Values.ToList();
        }

        public void Clear()
        {
            _cache.Clear();
        }
    }
}

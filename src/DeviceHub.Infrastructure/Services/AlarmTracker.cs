using DeviceHub.Core.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Infrastructure.Services
{
    /// <summary>
    /// 报警状态跟踪器：记录每个(设备,报警类型)的当前状态
    /// </summary>
    public class AlarmTracker
    {
        //(设备Id, 报警类型)→是否处于Active
        private readonly ConcurrentDictionary<(int DeviceId, AlarmType Type), bool> _states = new();

        /// <summary>
        /// 更新状态，返回是否发生了状态变化
        /// </summary>
        /// <returns>
        /// Normal → Active 返回 (true, false)：需触发报警
        /// Active → Normal 返回 (false, true)：需恢复
        /// 无变化返回 (false, false)
        /// </returns>
        public (bool Triggered, bool Resolved) Update(
            int deviceId, AlarmType type, bool isActive)
        {
            var key = (deviceId, type);
            var wasActive = _states.GetValueOrDefault(key, false);

            if (isActive && !wasActive)
            {
                //未触发→触发
                _states[key] = true;
                return (true, false);
            }

            if (!isActive && wasActive)
            {
                //触发→恢复
                _states[key] = false;
                return (false, true);
            }

            //无变化
            return (false, false);
        }

        /// <summary>
        /// 获取指定条件的当前状态
        /// </summary>
        public bool IsActive(int deviceId, AlarmType type)
            => _states.GetValueOrDefault((deviceId, type), false);

        /// <summary>
        /// 清空（用于测试）
        /// </summary>
        public void Clear() => _states.Clear();
    }
}

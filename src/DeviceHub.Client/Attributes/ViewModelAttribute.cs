using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Client.Attributes
{
    // <summary>
    /// 标记ViewModel的注册生命周期
    /// 默认按Transient注册
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ViewModelAttribute : Attribute
    {
        public ServiceLifetime Lifetime { get; }

        public ViewModelAttribute(ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            Lifetime = lifetime;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Exceptions
{
    /// <summary>
    /// 业务异常：消息面向用户，可直接显示
    /// </summary>
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message) { }
        public BusinessException(string message, Exception inner) : base(message, inner) { }
    }
}

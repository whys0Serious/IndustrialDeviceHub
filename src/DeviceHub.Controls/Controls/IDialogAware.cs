using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Controls.Controls
{
    /// <summary>
    /// 对话框ViewModel实现此接口以获得关闭对话框能力
    /// </summary>
    public interface IDialogAware
    {
        /// <summary>
        /// 请求关闭true = 确认false = 取消
        /// </summary>
        event Action<bool>? RequestClose;
    }
}

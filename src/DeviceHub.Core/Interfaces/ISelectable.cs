using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Interfaces
{
    public interface ISelectable
    {
        bool IsSelected { get; set; }
    }
}

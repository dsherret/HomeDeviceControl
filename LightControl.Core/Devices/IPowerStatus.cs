using System;

namespace LightControl.Core.Devices
{
    public interface IPowerStatus
    {
        event EventHandler PowerStatusChanged;
        bool PowerStatus { get; }
    }
}

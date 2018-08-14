using HomeDeviceControl.Communication.Common;
using System;

namespace HomeDeviceControl.Communication.Server
{
    /// <summary>
    /// Value listener for device power status events.
    /// </summary>
    public class DevicePowerStatusReceiver : IValueListener<DevicePowerStatus>
    {
        public class DevicePowerStatusChangedEventArgs : EventArgs
        {
            public DevicePowerStatus DevicePowerStatus { get; internal set; }
        }

        public event EventHandler<DevicePowerStatusChangedEventArgs> PowerStatusChanged;

        public string UrlRoute { get; } = Routes.DevicePowerStatus;

        public void OnValueReceived(DevicePowerStatus devicePowerStatus)
        {
            PowerStatusChanged?.Invoke(this, new DevicePowerStatusChangedEventArgs { DevicePowerStatus = devicePowerStatus });
        }
    }
}

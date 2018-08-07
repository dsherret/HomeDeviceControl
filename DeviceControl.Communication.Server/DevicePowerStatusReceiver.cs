using DeviceControl.Communication.Common;
using System;

namespace DeviceControl.Communication.Server
{
    public class DevicePowerStatusReceiver : IValueListener<DevicePowerStatus>
    {
        public class DevicePowerStatusChangedEventArgs : EventArgs
        {
            public DevicePowerStatus DevicePowerStatus { get; internal set; }
        }

        public DevicePowerStatusReceiver(string route)
        {
            UrlRoute = route;
        }

        public event EventHandler<DevicePowerStatusChangedEventArgs> PowerStatusChanged;

        public string UrlRoute { get; }

        public void OnValueReceived(DevicePowerStatus devicePowerStatus)
        {
            PowerStatusChanged?.Invoke(this, new DevicePowerStatusChangedEventArgs { DevicePowerStatus = devicePowerStatus });
        }
    }
}

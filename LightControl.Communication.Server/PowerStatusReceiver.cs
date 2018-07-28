using LightControl.Core.Devices;
using System;

namespace LightControl.Communication.Server
{
    public class PowerStatusReceiver : IPowerStatus, IValueListener<bool>
    {
        public PowerStatusReceiver(string route)
        {
            UrlRoute = route;
        }

        public event EventHandler PowerStatusChanged;

        public bool PowerStatus { get; private set; }

        public string UrlRoute { get; }

        public void OnValueReceived(bool isPoweredOn)
        {
            PowerStatus = isPoweredOn;
            PowerStatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

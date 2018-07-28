using LightControl.Core.Devices;
using System;
using System.Net.NetworkInformation;

namespace LightControl.Computer
{
    public sealed class ComputerStatus : IPowerStatus, IDisposable
    {
        private readonly object _lock = new object();
        private readonly IPowerStatus _serverPowerStatusReceiver;

        public ComputerStatus(IPowerStatus serverPowerStatusReceiver, string hostname)
        {
            _serverPowerStatusReceiver = serverPowerStatusReceiver;
            _serverPowerStatusReceiver.PowerStatusChanged += ServerPowerStatusReceiver_PowerStatusChanged;
            Initialize(hostname);
        }

        private async void Initialize(string hostname)
        {
            // don't care about race condition where power status could be set to false while pinging when shutting down
            var ping = new Ping();
            var result = await ping.SendPingAsync(hostname);
            PowerStatus = result.Status == IPStatus.Success;
        }

        public void Dispose()
        {
            _serverPowerStatusReceiver.PowerStatusChanged -= ServerPowerStatusReceiver_PowerStatusChanged;
        }

        public event EventHandler PowerStatusChanged;

        private bool _powerStatus;
        public bool PowerStatus
        {
            get
            {
                lock (_lock)
                    return _powerStatus;
            }
            private set
            {
                lock (_lock)
                {
                    if (_powerStatus == value)
                        return;

                    _powerStatus = value;
                }

                PowerStatusChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ServerPowerStatusReceiver_PowerStatusChanged(object sender, EventArgs e)
        {
            PowerStatus = _serverPowerStatusReceiver.PowerStatus;
        }
    }
}

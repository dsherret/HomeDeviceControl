using Microsoft.Win32;
using System;
using System.ServiceProcess;

namespace DeviceControl.WindowsEventService
{
    public partial class EventService : ServiceBase
    {
        public EventService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        }

        protected override void OnStop()
        {
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    SendPowerStatus(false);
                    break;
                case PowerModes.Suspend:
                    SendPowerStatus(true);
                    break;
            }
        }

        private async void SendPowerStatus(bool isPoweredOn)
        {
            // todo: don't hardcode
            const string computerDeviceId = "7d115c0c-6181-4965-bceb-449781ecd27a";
            const string serverUrl = "http://192.168.1.126:8084";

            using (var client = new Communication.ClientApi.Client(serverUrl))
                await client.UpdateDevicePowerStatus(new Guid(computerDeviceId), isPoweredOn);
        }
    }
}

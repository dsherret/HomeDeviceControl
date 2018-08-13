using DeviceControl.Core;
using Microsoft.Win32;
using System;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace DeviceControl.WindowsEventService
{
    public partial class EventService : ServiceBase
    {
        public EventService()
        {
            InitializeComponent();
        }

        internal void TestStart(string[] args)
        {
            OnStart(args);
        }

        internal void TestStop()
        {
            OnStop();
        }

        protected async override void OnStart(string[] args)
        {
            Logger.Log(this, LogLevel.Info, "Service starting...");
            await SendPowerStatusAsync(true);
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            Logger.Log(this, LogLevel.Info, "Service started.");
        }

        protected override void OnStop()
        {
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
            Logger.Log(this, LogLevel.Info, "Service stopped.");
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            try
            {
                // wait on the task to ensure the computer doesn't go into sleep mode before it's done
                SendStatusAsync().Wait();
            }
            catch (Exception ex)
            {
                Logger.Log(this, LogLevel.Error, ex);
                throw ex;
            }

            async Task SendStatusAsync()
            {
                switch (e.Mode)
                {
                    case PowerModes.Resume:
                        await SendPowerStatusAsync(true);
                        break;
                    case PowerModes.Suspend:
                        await SendPowerStatusAsync(false);
                        break;
                }
            }
        }

        private async Task SendPowerStatusAsync(bool isPoweredOn)
        {
            // todo: don't hardcode
            const string computerDeviceId = "7d115c0c-6181-4965-bceb-449781ecd27a";
            const string serverUrl = "http://192.168.1.125:8084";

            Logger.Log(this, LogLevel.Info, $"Sending power status: {isPoweredOn}.");

            using (var client = new Communication.ClientApi.Client(serverUrl))
                await client.UpdateDevicePowerStatus(new Guid(computerDeviceId), isPoweredOn);

            Logger.Log(this, LogLevel.Info, $"Sent power status: {isPoweredOn}.");
        }
    }
}

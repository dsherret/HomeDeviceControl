using HomeDeviceControl.Core;
using System;
using System.Management;

namespace HomeDeviceControl.Communication.PowerStatus.WindowsService
{
    public class PowerEventListener
    {
        private readonly ManagementEventWatcher managementEventWatcher;
        private bool _isSubscribed = false;

        public PowerEventListener()
        {
            // from here: https://stackoverflow.com/a/9159974/188246
            // much more reliable than SystemEvents.PowerModeChanged
            var query = new WqlEventQuery();
            var scope = new ManagementScope("root\\CIMV2");

            query.EventClassName = "Win32_PowerManagementEvent";
            managementEventWatcher = new ManagementEventWatcher(scope, query);
        }

        public void Start()
        {
            if (_isSubscribed)
                return;

            managementEventWatcher.EventArrived += PowerEventArrived;
            managementEventWatcher.Start();
            _isSubscribed = true;
            Logger.Log(this, LogLevel.Info, "Subscribed to power mode changed event.");
        }

        public void Stop()
        {
            if (!_isSubscribed)
                return;

            managementEventWatcher.EventArrived -= PowerEventArrived;
            managementEventWatcher.Stop();
            _isSubscribed = false;
            Logger.Log(this, LogLevel.Info, "Unsubscribed from power mode changed event.");
        }

        private async void PowerEventArrived(object sender, EventArrivedEventArgs e)
        {
            const ushort SUSPEND_EVENT = 4;
            const ushort RESUME_EVENT = 7;

            foreach (PropertyData pd in e.NewEvent.Properties)
            {
                var value = Convert.ToUInt16(pd?.Value);

                switch (value)
                {
                    case SUSPEND_EVENT:
                        await PowerStatus.SendAsync(false);
                        break;
                    case RESUME_EVENT:
                        await PowerStatus.SendAsync(true);
                        break;
                }
            }
        }
    }
}

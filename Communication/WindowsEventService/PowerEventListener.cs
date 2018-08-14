using HomeDeviceControl.Core;
using System.Management;

namespace HomeDeviceControl.Communication.WindowsEventService
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
            const string SUSPEND_EVENT = "4";
            const string RESUME_EVENT = "7";

            foreach (PropertyData pd in e.NewEvent.Properties)
            {
                switch (pd?.Value?.ToString())
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

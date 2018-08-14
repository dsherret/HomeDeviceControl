using HomeDeviceControl.Core;
using System;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;

namespace HomeDeviceControl.Communication.WindowsEventService
{
    public partial class EventService : ServiceBase
    {
        private Settings _settings;

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
            _settings = new ArgumentsParser().ParseArguments(args);
            Logger.Log(this, LogLevel.Info, "Service starting...");
            await PowerStatus.SendAsync(_settings, true);
            new Thread(RunMessagePump).Start();
            Logger.Log(this, LogLevel.Info, "Service started.");
        }

        protected override void OnStop()
        {
            Logger.Log(this, LogLevel.Info, "Service stopped.");
            Application.Exit();
        }

        private void RunMessagePump()
        {
            Application.Run(new HiddenForm(_settings));
        }
    }
}

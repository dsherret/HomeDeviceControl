using DeviceControl.Core;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Management;
using System.Windows.Forms;

namespace DeviceControl.WindowsEventService
{
    // This is necessary to create a message loop in order to listen to events on SystemEvents.
    // See https://msdn.microsoft.com/en-us/library/microsoft.win32.systemevents(v=vs.110).aspx for more details.

    public partial class HiddenForm : Form
    {
        private ManagementEventWatcher managementEventWatcher;

        public HiddenForm()
        {
            InitializeComponent();
        }

        private void HiddenForm_Load(object sender, EventArgs e)
        {
            Logger.Log(this, LogLevel.Info, "Subscribed to power mode changed event.");

            // from here: https://stackoverflow.com/a/9159974/188246
            // much more reliable than SystemEvents.PowerModeChanged
            var q = new WqlEventQuery();
            var scope = new ManagementScope("root\\CIMV2");

            q.EventClassName = "Win32_PowerManagementEvent";
            managementEventWatcher = new ManagementEventWatcher(scope, q);
            managementEventWatcher.EventArrived += PowerEventArrived;
            managementEventWatcher.Start();

            Logger.Log(this, LogLevel.Info, "Subscribed to power mode changed event.");
        }

        private void HiddenForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            managementEventWatcher?.Stop();
            Logger.Log(this, LogLevel.Info, "Unsubscribed from power mode changed event.");
        }

        private async void PowerEventArrived(object sender, EventArrivedEventArgs e)
        {
            foreach (PropertyData pd in e.NewEvent.Properties)
            {
                switch (pd?.Value?.ToString())
                {
                    case "4":
                        await PowerStatus.SendAsync(false);
                        break;
                    case "7":
                        await PowerStatus.SendAsync(true);
                        break;
                }
            }
        }
    }

    partial class HiddenForm
    {
        private IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                components?.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(0, 0);
            FormBorderStyle = FormBorderStyle.None;
            Name = "HiddenForm";
            Text = "HiddenForm";
            WindowState = FormWindowState.Minimized;
            Load += new EventHandler(HiddenForm_Load);
            FormClosing += new FormClosingEventHandler(HiddenForm_FormClosing);
            ResumeLayout(false);
        }
    }
}

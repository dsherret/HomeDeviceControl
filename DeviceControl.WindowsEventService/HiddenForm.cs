using DeviceControl.Core;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeviceControl.WindowsEventService
{
    // This is necessary to create a message loop in order to listen to events on SystemEvents.
    // See https://msdn.microsoft.com/en-us/library/microsoft.win32.systemevents(v=vs.110).aspx for more details.

    public partial class HiddenForm : Form
    {
        public HiddenForm()
        {
            InitializeComponent();
        }

        private void HiddenForm_Load(object sender, EventArgs e)
        {
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            Logger.Log(this, LogLevel.Info, "Subscribed to power mode changed event.");
        }

        private void HiddenForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
            Logger.Log(this, LogLevel.Info, "Unsubscribed from power mode changed event.");
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
                        await PowerStatus.SendAsync(true);
                        break;
                    case PowerModes.Suspend:
                        await PowerStatus.SendAsync(false);
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

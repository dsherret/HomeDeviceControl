using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace HomeDeviceControl.Communication.WindowsEventService
{
    // This is necessary to create a message loop in order to listen to events on SystemEvents.
    // See https://msdn.microsoft.com/en-us/library/microsoft.win32.systemevents(v=vs.110).aspx for more details.

    public partial class HiddenForm : Form
    {
        private PowerEventListener _powerEventListener;
        public HiddenForm()
        {
            InitializeComponent();
        }

        private void HiddenForm_Load(object sender, EventArgs e)
        {
            _powerEventListener = new PowerEventListener();
            _powerEventListener.Start();
        }

        private void HiddenForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _powerEventListener?.Stop();
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

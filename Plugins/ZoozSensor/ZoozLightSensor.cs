using HomeDeviceControl.Core.Sensors;
using System;
using ZWave.CommandClasses;
using HomeDeviceControl.ZWave;
using HomeDeviceControl.Core;
using HomeDeviceControl.Core.Utils;

namespace HomeDeviceControl.Plugin.ZoozSensor
{
    public sealed class ZoozLightSensor : ILightSensor
    {
        private readonly object _lock = new object();
        private readonly ZWaveController _controller;
        private readonly string _port;
        private readonly int _nodeId;
        private SensorMultiLevel _sensorCommand;

        public ZoozLightSensor(string port, int nodeId)
        {
            _port = port;
            _nodeId = nodeId;
            _controller = ZWaveControllerFactory.Instance.AcquireController(port);
            _controller.Connected += Controller_Connected;
        }

        public void Dispose()
        {
            lock (_lock)
                TryUnsubscribeSensorCommand(_sensorCommand);
            ZWaveControllerFactory.Instance.ReleaseController(_port);
        }

        public event EventHandler<LuminanceChangedEventArgs> LuminanceChanged;

        private async void Controller_Connected(object sender, EventArgs e)
        {
            var node = await _controller.GetNodeAsync(_nodeId);

            lock (_lock)
            {
                TryUnsubscribeSensorCommand(_sensorCommand);

                if (node == null)
                    return;

                _sensorCommand = node.GetCommandClass<SensorMultiLevel>();
                _sensorCommand.Changed += SensorCommand_Changed;
                Logger.Log(this, LogLevel.Info, "Subscribed to new light sensor.");
            }
        }

        private void TryUnsubscribeSensorCommand(SensorMultiLevel command)
        {
            if (command == null)
                return;

            ActionUtils.TryDoAction(() => command.Changed -= SensorCommand_Changed, "Error unsubscribing from light sensor.");
        }

        private void SensorCommand_Changed(object sender, ReportEventArgs<SensorMultiLevelReport> e)
        {
            if (e.Report.Type == SensorType.Luminance)
                LuminanceChanged?.Invoke(this, new LuminanceChangedEventArgs(e.Report.Value));
        }
    }
}

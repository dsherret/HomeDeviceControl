using LightControl.Core.Sensors;
using ZWave;
using System;
using ZWave.CommandClasses;
using System.Threading.Tasks;
using LightControl.ZWave;

namespace LightControl.Plugin.ZoozSensor
{
    public sealed class ZoozLightSensor : ILightSensor
    {
        private readonly ZWaveController _controller;
        private SensorMultiLevel _sensorCommand;

        public ZoozLightSensor()
        {
            _controller = ZWaveControllerFactory.Instance.AcquireController();
        }

        public void Dispose()
        {
            _sensorCommand.Changed -= SensorCommand_Changed;
            _controller.Close();
        }

        public event EventHandler<LuminanceChangedEventArgs> LuminanceChanged;

        public async Task SetupAsync()
        {
            if (_sensorCommand != null)
                return;

            var nodes = await _controller.GetNodes();
            var node = nodes[5];
            _sensorCommand = node.GetCommandClass<SensorMultiLevel>();
            _sensorCommand.Changed += SensorCommand_Changed;
        }

        private void SensorCommand_Changed(object sender, ReportEventArgs<SensorMultiLevelReport> e)
        {
            if (e.Report.Type == SensorType.Luminance)
                LuminanceChanged?.Invoke(this, new LuminanceChangedEventArgs(e.Report.Value));
        }
    }
}

using System;

namespace DeviceControl.Core.Sensors
{
    public interface IMotionSensor : ISensor
    {
        event EventHandler MotionDetected;
    }
}

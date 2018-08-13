using System;

namespace HomeDeviceControl.Core.Sensors
{
    public interface IMotionSensor : ISensor
    {
        event EventHandler MotionDetected;
    }
}

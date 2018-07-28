using System;

namespace LightControl.Core.Sensors
{
    public interface IMotionSensor : ISensor
    {
        event EventHandler MotionDetected;
    }
}

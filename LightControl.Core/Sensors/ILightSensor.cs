using System;

namespace LightControl.Core.Sensors
{
    public class LuminanceChangedEventArgs : EventArgs
    {
        public LuminanceChangedEventArgs(float value)
        {
            Value = value;
        }

        /// <summary>
        /// A luminance percent value between 0 and 100.
        /// </summary>
        public float Value { get; }
    }

    public interface ILightSensor : ISensor
    {
        event EventHandler<LuminanceChangedEventArgs> LuminanceChanged;
    }
}

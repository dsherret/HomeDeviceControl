using System;

namespace LightControl.Communication.Common
{
    public class DevicePowerStatus
    {
        public Guid DeviceId { get; set; }
        public bool IsPoweredOn { get; set; }
    }
}

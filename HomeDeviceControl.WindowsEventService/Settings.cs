using System;

namespace HomeDeviceControl.WindowsEventService
{
    /// <summary>
    /// Settings provided by arguments.
    /// </summary>
    public struct Settings
    {
        public Guid ComputerDeviceId { get; set; }
        public string ServerUrl { get; set; }
    }
}

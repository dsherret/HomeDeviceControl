using System;

namespace DeviceControl.Communication.Server
{
    public interface IValueListener<in T>
    {
        string UrlRoute { get; }
        void OnValueReceived(T value);
    }
}

using System;

namespace LightControl.Communication.Server
{
    public interface IValueListener<in T>
    {
        string UrlRoute { get; }
        void OnValueReceived(T value);
    }
}

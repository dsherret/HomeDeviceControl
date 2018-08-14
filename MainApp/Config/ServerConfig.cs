using HomeDeviceControl.Communication.Server;

namespace HomeDeviceControl.MainApp.Config
{
    public static class ServerConfig
    {
        public static Server Setup(HomeContext context)
        {
            var server = new Server(Settings.Default.ServerHostname, Settings.Default.ServerPort);
            server.AddValueListener(context.DevicePowerStatusReceiver);
            return server;
        }
    }
}

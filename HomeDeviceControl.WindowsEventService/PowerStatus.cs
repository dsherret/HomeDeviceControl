using HomeDeviceControl.Core;
using System;
using System.Threading.Tasks;

namespace HomeDeviceControl.WindowsEventService
{
    public static class PowerStatus
    {
        public static async Task SendAsync(bool isPoweredOn)
        {
            // todo: don't hardcode
            const string computerDeviceId = "7d115c0c-6181-4965-bceb-449781ecd27a";
            const string serverUrl = "http://192.168.1.125:8084";

            Logger.Log(typeof(PowerStatus), LogLevel.Info, $"Sending power status: {isPoweredOn}.");

            using (var client = new Communication.ClientApi.Client(serverUrl))
                await client.UpdateDevicePowerStatus(new Guid(computerDeviceId), isPoweredOn);

            Logger.Log(typeof(PowerStatus), LogLevel.Info, $"Sent power status: {isPoweredOn}.");
        }
    }
}

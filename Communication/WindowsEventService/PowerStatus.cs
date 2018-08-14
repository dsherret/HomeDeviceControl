using HomeDeviceControl.Core;
using System.Threading.Tasks;

namespace HomeDeviceControl.Communication.WindowsEventService
{
    public static class PowerStatus
    {
        public static async Task SendAsync(Settings settings, bool isPoweredOn)
        {
            Logger.Log(typeof(PowerStatus), LogLevel.Info, $"Sending power status: {isPoweredOn}.");

            using (var client = new Communication.ClientApi.Client(settings.ServerUrl))
                await client.UpdateDevicePowerStatus(settings.ComputerDeviceId, isPoweredOn);

            Logger.Log(typeof(PowerStatus), LogLevel.Info, $"Sent power status: {isPoweredOn}.");
        }
    }
}

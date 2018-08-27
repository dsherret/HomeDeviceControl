using HomeDeviceControl.Core;
using System;
using System.Threading.Tasks;

namespace HomeDeviceControl.Communication.PowerStatus.WindowsService
{
    public static class PowerStatus
    {
        public static async Task SendAsync(bool isPoweredOn)
        {
            // todo: abstract out retrying
            const int MAX_RETRIES = 10;
            for (var i = 0; i < MAX_RETRIES; i++)
            {
                if (await SendInternalAsync())
                    return;
                await Task.Delay(100 * (i + 1));
            }

            async Task<bool> SendInternalAsync()
            {
                Logger.Log(typeof(PowerStatus), LogLevel.Info, $"Sending power status: {isPoweredOn}.");

                try
                {
                    using (var client = new ClientApi.Client(Settings.Default.ServerUrl))
                        await client.UpdateDevicePowerStatus(Guid.Parse(Settings.Default.ComputerDeviceId), isPoweredOn);

                    Logger.Log(typeof(PowerStatus), LogLevel.Info, $"Sent power status: {isPoweredOn}.");
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Log(typeof(PowerStatus), LogLevel.Error, "Problem sending power status.", ex);
                    return false;
                }
            }
        }
    }
}

using HomeDeviceControl.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HomeDeviceControl.Communication.PowerStatus.WindowsService
{
    public static class PowerStatus
    {
        private readonly static object _syncLock = new object();
        private static CancellationTokenSource _cts;

        public static async Task SendAsync(bool isPoweredOn)
        {
            var token = GetNewToken();
            try
            {
                await SendWithRetryAndCancellation();
            }
            catch (OperationCanceledException)
            {
                // do nothing
            }

            async Task SendWithRetryAndCancellation()
            {
                // todo: abstract out retrying
                const int MAX_RETRIES = 10;
                for (var i = 0; i < MAX_RETRIES; i++)
                {
                    if (await SendInternalAsync())
                        return;
                    await Task.Delay(100 * (i + 1), token);
                }
            }

            async Task<bool> SendInternalAsync()
            {
                Logger.Log(typeof(PowerStatus), LogLevel.Info, $"Sending power status: {isPoweredOn}.");

                try
                {
                    using (var client = new ClientApi.Client(Settings.Default.ServerUrl))
                        await client.UpdateDevicePowerStatus(Guid.Parse(Settings.Default.ComputerDeviceId), isPoweredOn, token);

                    Logger.Log(typeof(PowerStatus), LogLevel.Info, $"Sent power status: {isPoweredOn}.");
                    return true;
                }
                catch (OperationCanceledException)
                {
                    Logger.Log(typeof(PowerStatus), LogLevel.Info, "Sending power status cancelled.");
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.Log(typeof(PowerStatus), LogLevel.Error, "Problem sending power status.", ex);
                    return false;
                }
            }
        }

        private static CancellationToken GetNewToken()
        {
            lock (_syncLock)
            {
                if (_cts != null)
                {
                    _cts.Cancel();
                    _cts.Dispose();
                }
                _cts = new CancellationTokenSource();
                return _cts.Token;
            }
        }
    }
}

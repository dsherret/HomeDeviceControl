﻿using HomeDeviceControl.Core;
using System;
using System.Threading.Tasks;

namespace HomeDeviceControl.Communication.PowerStatus.WindowsService
{
    public static class PowerStatus
    {
        public static async Task SendAsync(bool isPoweredOn)
        {
            Logger.Log(typeof(PowerStatus), LogLevel.Info, $"Sending power status: {isPoweredOn}.");

            try
            {
                using (var client = new ClientApi.Client(Settings.Default.ServerUrl))
                    await client.UpdateDevicePowerStatus(Guid.Parse(Settings.Default.ComputerDeviceId), isPoweredOn);

                Logger.Log(typeof(PowerStatus), LogLevel.Info, $"Sent power status: {isPoweredOn}.");
            }
            catch (Exception ex)
            {
                Logger.Log(typeof(PowerStatus), LogLevel.Error, "Problem sending power status.", ex);
            }
        }
    }
}

using HomeDeviceControl.Communication.Server;
using HomeDeviceControl.Core;
using HomeDeviceControl.Core.Utils;
using HomeDeviceControl.LightBulbs;
using HomeDeviceControl.Plugin.ZoozSensor;
using System;
using System.Threading.Tasks;

namespace HomeDeviceControl.Config
{
    public static class SunroomConfig
    {
        public static void Setup(HomeContext homeContext)
        {
            SetupStateChangers(homeContext);
            Config(homeContext);
        }

        private static void SetupStateChangers(HomeContext homeContext)
        {
            var motionSensor = new ZoozMotionSensor(Settings.Default.ZWavePort, Settings.Default.SunroomZWaveZoozNodeId);
            var lightSensor = new ZoozLightSensor(Settings.Default.ZWavePort, Settings.Default.SunroomZWaveZoozNodeId);
            var homeStateContainer = homeContext.HomeStateContainer;

            lightSensor.LuminanceChanged += (sender, e) =>
            {
                Logger.Log(typeof(SunroomConfig), LogLevel.Info, $"Sunroom luminance changed: {e.Value}");
                homeStateContainer.UpdateState(state => state.Sunroom.Luminance = (int)e.Value);
            };

            motionSensor.MotionDetected += (sender, e) =>
            {
                Logger.Log(typeof(SunroomConfig), LogLevel.Info, "Sunroom motion detected.");
                homeStateContainer.UpdateState(state => state.Sunroom.LastMotionDetected = DateTime.Now);
            };

            homeContext.DevicePowerStatusReceiver.PowerStatusChanged += (sender, e) =>
            {
                if (e.DevicePowerStatus.DeviceId == Settings.Default.SunroomComputerId)
                {
                    Logger.Log(typeof(SunroomConfig), LogLevel.Info, $"Sunroom computer power status changed ({e.DevicePowerStatus.IsPoweredOn})");
                    homeStateContainer.UpdateState(state => state.Sunroom.IsComputerOn = e.DevicePowerStatus.IsPoweredOn);
                }
            };
        }

        private static void Config(HomeContext homeContext)
        {
            var sunRoomBulb = homeContext.LightBulbFactory.Get(Settings.Default.SunroomLightId);
            var dimmedActioner = GetDimmedActioner(sunRoomBulb, homeContext);

            homeContext.HomeStateContainer.OnStateUpdated(async (e) =>
            {
                var oldState = e.OldState.Sunroom;
                var newState = e.NewState.Sunroom;

                // sync light power state with computer state
                if (oldState.IsComputerOn != newState.IsComputerOn)
                {
                    if (newState.IsComputerOn)
                    {
                        if (newState.IsDark)
                        {
                            Logger.Log(typeof(SunroomConfig), LogLevel.Info, $"Sunroom computer turned on. Ensuring light is on since it's dark.");
                            await sunRoomBulb.SetPowerAsync(true);
                        }
                        homeContext.LightBulbBrightnessController.HandleLightBulb(sunRoomBulb);
                    }
                    else
                    {
                        if (newState.IsDark)
                        {
                            Logger.Log(typeof(SunroomConfig), LogLevel.Info, $"Sunroom computer turned off. Dimming light while motion is detected.");
                            await dimmedActioner.DoActions();
                        }
                        else
                        {
                            if (await sunRoomBulb.GetPowerAsync())
                            {
                                Logger.Log(typeof(SunroomConfig), LogLevel.Info, $"Sunroom computer turned off. Turning off light.");
                                await sunRoomBulb.SetPowerAsync(false);
                            }
                        }
                    }
                }

                // turn on the light if the computer is on and it's become dark
                if (!oldState.IsDark && newState.IsDark && newState.IsComputerOn)
                {
                    Logger.Log(typeof(SunroomConfig), LogLevel.Info, $"Sunroom is dark and computer is on. Turning light on.");
                    await sunRoomBulb.SetPowerAsync(true);
                }

                // when it detects motion while the computer is off, turn on the light if necessary
                if (oldState.LastMotionDetected != newState.LastMotionDetected && !newState.IsComputerOn && newState.IsDark)
                {
                    if (!(await sunRoomBulb.GetPowerAsync()))
                    {

                        Logger.Log(typeof(SunroomConfig), LogLevel.Info,
                            $"Sunroom is dark, computer is off, and motion was detected. Turning light dimly on.");
                        await dimmedActioner.DoActions();
                    }
                }
            });
        }

        private static StartResetableEndActioner GetDimmedActioner(LightBulbWrapper sunRoomBulb, HomeContext homeContext)
        {
            return new StartResetableEndActioner(async () =>
            {
                homeContext.LightBulbBrightnessController.UnhandleLightBulb(sunRoomBulb);
                await Task.WhenAll(
                    sunRoomBulb.SetPowerAsync(true),
                    sunRoomBulb.SetBrightnessAsync(Settings.Default.SunroomMotionDimmedBrightness)
                    );
            }, async () =>
            {
                await homeContext.CoordinationLock.DoActionAsync(async () => {
                    var powerStatus = await sunRoomBulb.GetPowerAsync();
                    var brightness = await sunRoomBulb.GetBrightnessAsync();

                    // only turn off if its in the state it was set to originally
                    if (powerStatus && brightness == Settings.Default.SunroomMotionDimmedBrightness)
                    {
                        Logger.Log(typeof(SunroomConfig), LogLevel.Info, $"Turning off sunroom's dimly lit bulb since no motion has been detected.");
                        await sunRoomBulb.SetPowerAsync(false);
                    }
                    else
                    {
                        Logger.Log(typeof(SunroomConfig), LogLevel.Info, $"Sunroom bulb has changed state since being dimly lit so state will be left alone.");
                    }

                    homeContext.LightBulbBrightnessController.HandleLightBulb(sunRoomBulb);
                });
            }, () => TimeSpan.FromSeconds(Settings.Default.SunroomMotionDimmedSeconds));
        }
    }
}

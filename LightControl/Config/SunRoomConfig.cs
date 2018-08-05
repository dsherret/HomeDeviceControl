using LightControl.Communication.Server;
using LightControl.Core.Utils;
using LightControl.LightBulbs;
using LightControl.Plugin.ZoozSensor;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace LightControl.Config
{
    public static class SunRoomConfig
    {
        public static void Setup(HomeContext homeContext)
        {
            SetupStateChangers(homeContext);
            Config(homeContext);
        }

        private static void SetupStateChangers(HomeContext homeContext)
        {
            var motionSensor = new ZoozMotionSensor(DeviceIdentifiers.SunRoomZoozSensorNodeId);
            var lightSensor = new ZoozLightSensor(DeviceIdentifiers.SunRoomZoozSensorNodeId);
            var homeStateContainer = homeContext.HomeStateContainer;

            lightSensor.LuminanceChanged += (sender, e) => homeStateContainer.UpdateState(state => state.SunRoom.Luminance = (int)e.Value);

            motionSensor.MotionDetected += (sender, e) =>
            {
                Console.WriteLine("Motion detected.");
                homeStateContainer.UpdateState(state => state.SunRoom.LastMotionDetected = DateTime.Now);
            };

            homeContext.DevicePowerStatusReceiver.PowerStatusChanged += (sender, e) =>
            {
                if (e.DevicePowerStatus.DeviceId == DeviceIdentifiers.ComputerId)
                    homeStateContainer.UpdateState(state => state.SunRoom.IsComputerOn = e.DevicePowerStatus.IsPoweredOn);
            };
        }

        private static void Config(HomeContext homeContext)
        {
            var sunRoomBulb = homeContext.LightBulbFactory.Get(DeviceIdentifiers.SunroomLightId);
            var dimmedActioner = GetDimmedActioner(sunRoomBulb, homeContext);

            homeContext.HomeStateContainer.OnStateUpdated(async (e) =>
            {
                var oldState = e.OldState.SunRoom;
                var newState = e.NewState.SunRoom;

                // sync light power state with computer state
                if (oldState.IsComputerOn != newState.IsComputerOn)
                {
                    if (!newState.IsComputerOn)
                    {
                        if (newState.IsDark)
                        {
                            await dimmedActioner.DoActions();
                        }
                        else
                            await sunRoomBulb.SetPowerAsync(false);
                    }
                    else
                    {
                        await sunRoomBulb.SetPowerAsync(newState.IsDark);
                        homeContext.LightBulbColorController.HandleLightBulb(sunRoomBulb);
                    }
                }

                if (!oldState.IsDark && newState.IsDark && newState.IsComputerOn)
                    await sunRoomBulb.SetPowerAsync(true);

                // when it detects motion while the computer is off, turn on the light if necessary
                if (oldState.LastMotionDetected != newState.LastMotionDetected && !newState.IsComputerOn && newState.IsDark)
                {
                    if (!(await sunRoomBulb.GetPowerAsync()))
                        await dimmedActioner.DoActions();
                }
            });
        }

        private static StartResetableEndActioner GetDimmedActioner(LightBulbWrapper sunRoomBulb, HomeContext homeContext)
        {
            var fadeColor = Color.FromArgb(255, 25, 25);
            var fadeBrightness = 10;
            return new StartResetableEndActioner(async () =>
            {
                homeContext.LightBulbColorController.UnhandleLightBulb(sunRoomBulb);
                await Task.WhenAll(
                    sunRoomBulb.SetPowerAsync(true),
                    sunRoomBulb.SetBrightnessAsync(fadeBrightness),
                    sunRoomBulb.SetColorAsync(fadeColor)
                    );
            }, async () =>
            {
                await homeContext.CoordinationLock.DoActionAsync(async () => {
                    var powerStatus = await sunRoomBulb.GetPowerAsync();
                    var brightness = await sunRoomBulb.GetBrightnessAsync();
                    var color = await sunRoomBulb.GetColorAsync();

                    // only turn off if its in the state it was set to originally
                    if (powerStatus && brightness == fadeBrightness && color == fadeColor)
                        await sunRoomBulb.SetPowerAsync(false);

                    homeContext.LightBulbColorController.HandleLightBulb(sunRoomBulb);
                });
            }, TimeSpan.FromSeconds(15));
        }
    }
}

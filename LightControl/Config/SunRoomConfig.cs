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
        public static void Setup(HomeStateContainer homeState, LightBulbFactory lightBulbs, DevicePowerStatusReceiver devicePowerStatusReceiver)
        {
            SetupStateChangers(homeState, devicePowerStatusReceiver);
            Config(homeState, lightBulbs);
        }

        private static void SetupStateChangers(HomeStateContainer homeState, DevicePowerStatusReceiver devicePowerStatusReceiver)
        {
            var motionSensor = new ZoozMotionSensor(DeviceIdentifiers.SunRoomZoozSensorNodeId);
            var lightSensor = new ZoozLightSensor(DeviceIdentifiers.SunRoomZoozSensorNodeId);

            lightSensor.LuminanceChanged += (sender, e) => homeState.UpdateState(state => state.SunRoom.Luminance = (int)e.Value);

            motionSensor.MotionDetected += (sender, e) =>
            {
                Console.WriteLine("Motion detected.");
                homeState.UpdateState(state => state.SunRoom.LastMotionDetected = DateTime.Now);
            };

            devicePowerStatusReceiver.PowerStatusChanged += (sender, e) =>
            {
                if (e.DevicePowerStatus.DeviceId == DeviceIdentifiers.ComputerId)
                    homeState.UpdateState(state => state.SunRoom.IsComputerOn = e.DevicePowerStatus.IsPoweredOn);
            };
        }

        private static void Config(HomeStateContainer homeState, LightBulbFactory lightBulbs)
        {
            var sunRoomBulb = lightBulbs.Get(DeviceIdentifiers.SunroomLightId);
            var fadeColor = Color.FromArgb(255, 25, 25);
            var fadeBrightness = 10;
            var onAndOffAction = new StartResetableEndActioner(async () =>
            {
                if (!(await sunRoomBulb.GetPowerAsync()))
                {
                    await Task.WhenAll(
                        sunRoomBulb.SetPowerAsync(true),
                        sunRoomBulb.SetBrightnessAsync(fadeBrightness),
                        sunRoomBulb.SetColorAsync(fadeColor)
                        );
                }
            }, async () =>
            {
                var powerStatus = await sunRoomBulb.GetPowerAsync();
                var brightness = await sunRoomBulb.GetBrightnessAsync();
                var color = await sunRoomBulb.GetColorAsync();

                // only turn off if its in the state it was set to originally
                if (powerStatus && brightness == fadeBrightness && color == fadeColor)
                    await sunRoomBulb.SetPowerAsync(false);
            }, TimeSpan.FromSeconds(15));

            homeState.StateUpdated += async (sender, e) =>
            {
                var oldState = e.OldState.SunRoom;
                var newState = e.NewState.SunRoom;

                // sync light power state with computer state
                if (oldState.IsComputerOn != newState.IsComputerOn)
                    await sunRoomBulb.SetPowerAsync(newState.IsComputerOn);

                // when it detects motion while the computer is off, turn on the light if necessary
                if (oldState.LastMotionDetected != newState.LastMotionDetected && !newState.IsComputerOn && newState.Luminance < 30)
                    await onAndOffAction.DoActions();
            };
        }
    }
}

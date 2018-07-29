using LightControl.Communication.Server;
using LightControl.LightBulbs;
using LightControl.Plugin.ZoozSensor;
using System;
using System.Drawing;
using System.Threading;
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

            motionSensor.MotionDetected += (sender, e) => homeState.UpdateState(state => state.SunRoom.LastMotionDetected = DateTime.Now);

            devicePowerStatusReceiver.PowerStatusChanged += (sender, e) =>
            {
                if (e.DevicePowerStatus.DeviceId == DeviceIdentifiers.ComputerId)
                    homeState.UpdateState(state => state.SunRoom.IsComputerOn = e.DevicePowerStatus.IsPoweredOn);
            };
        }

        private static void Config(HomeStateContainer homeState, LightBulbFactory lightBulbs)
        {
            var sunRoomBulb = lightBulbs.Get(DeviceIdentifiers.SunroomLightId);
            var semaphore = new SemaphoreSlim(1);
            CancellationTokenSource cts = null;

            homeState.StateUpdated += async (sender, e) =>
            {
                var oldState = e.OldState.SunRoom;
                var newState = e.NewState.SunRoom;

                // sync light power state with computer state
                if (oldState.IsComputerOn != newState.IsComputerOn)
                    await sunRoomBulb.SetPowerAsync(newState.IsComputerOn);

                // when it detects motion while the computer is off, turn on the light if necessary
                if (oldState.LastMotionDetected != newState.LastMotionDetected && !newState.IsComputerOn && newState.Luminance < 30)
                {
                    // todo: refactor out to simplify and reuse this logic
                    bool shouldDoPowerOffTask = false;
                    await semaphore.WaitAsync();
                    try
                    {
                        if (cts != null)
                        {
                            cts.Cancel();
                            cts.Dispose();
                            cts = null;
                            shouldDoPowerOffTask = true;
                        }

                        if (!(await sunRoomBulb.GetPowerAsync()))
                        {
                            shouldDoPowerOffTask = true;
                            await Task.WhenAll(
                                sunRoomBulb.SetPowerAsync(true),
                                sunRoomBulb.SetBrightnessAsync(10),
                                sunRoomBulb.SetColorAsync(Color.FromArgb(255, 25, 25))
                                );
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }

                    if (shouldDoPowerOffTask)
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            cts = new CancellationTokenSource();
                            var token = cts.Token;
                            var unusedTask = Task.Run(async () =>
                            {
                                await Task.Delay(3_000, token);
                                await semaphore.WaitAsync();
                                try
                                {
                                    token.ThrowIfCancellationRequested();

                                    var powerStatus = await sunRoomBulb.GetPowerAsync();
                                    var brightness = await sunRoomBulb.GetBrightnessAsync();
                                    var color = await sunRoomBulb.GetColorAsync();

                                    // only turn off if its in the state it was set to originally
                                    if (powerStatus && brightness == 10 && color == Color.FromArgb(255, 25, 25))
                                        await sunRoomBulb.SetPowerAsync(false);

                                    cts.Dispose();
                                    cts = null;
                                }
                                finally
                                {
                                    semaphore.Release();
                                }
                            }, token);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }
                }
            };
        }
    }
}

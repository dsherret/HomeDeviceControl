using HomeDeviceControl.Communication.Common;
using HomeDeviceControl.Communication.Server;
using HomeDeviceControl.Core;
using HomeDeviceControl.Core.Utils;
using HomeDeviceControl.LightBulbs;

namespace HomeDeviceControl
{
    public class HomeContext
    {
        public HomeContext()
        {
            CoordinationLock = new AsyncCoordinationLock();
            var pluginSystem = new PluginSystem();

            DevicePowerStatusReceiver = new DevicePowerStatusReceiver(Routes.DevicePowerStatus);
            HomeStateContainer = new HomeStateContainer(CoordinationLock);
            LightBulbFactory = new LightBulbFactory(pluginSystem.LightBulbStore);
            LightBulbColorController = new LightBulbColorController(HomeStateContainer);
            LightBulbBrightnessController = new LightBulbBrightnessController(HomeStateContainer);

            LightBulbFactory.Added += (sender, e) =>
            {
                // start watching the bulb for color and brightness changes
                LightBulbColorController.HandleLightBulb(e.Value);
                LightBulbBrightnessController.HandleLightBulb(e.Value);
            };
        }

        public LightBulbFactory LightBulbFactory { get; }

        public AsyncCoordinationLock CoordinationLock { get; }

        public LightBulbColorController LightBulbColorController { get; }

        public LightBulbBrightnessController LightBulbBrightnessController { get; }

        public HomeStateContainer HomeStateContainer { get; }

        public DevicePowerStatusReceiver DevicePowerStatusReceiver { get; }
    }
}

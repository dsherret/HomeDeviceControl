# Home Device Control

This is the code used to control my home devices. It is specific to my situation and **not designed for reuse**, but feel free to fork and modify the code for your use.

I created this because I didn't think it was possible to get as good of an experience with existing solutions. I also wanted to ensure I'd be able to easily integrate any device I bought.

## Overview

### Light Bulbs

* Temperature, brightness, and colour of the bulbs adjust based on sun altitude, time, and if configured the room's luminance (requires a luminance detector, which I have in the sunroom). Similar idea to [flux](https://justgetflux.com/), but for the lights in my home.
* At 11:00 PM the lights turn red and stay that way until I turn them off or sunrise occurs.
* Bulbs are automatically detected (zero configuration).
    * Discovery happens on application startup.
    * For a light bulb initially gaining power (ex. being turned on via a non-smart lightswitch), it will connect and then broadcast its status on the network. Its broadcast will then be picked up and it will be sent the correct temperature, colour, and brightness based on the current time. Unfortunately this process takes about 8 seconds.
* Any property of the lights can be manually overridden and the system won't automatically adjust that property until the power has been turned on and off.
    * For example, I could change the colour of a light bulb to green using the device's mobile app and it would stay green. Then if I wanted to return to the system's automatic colour control, I only need to turn the light bulb on and off using the mobile app or a light switch.
* Currently only supports [Yeelight bulbs](https://www.amazon.ca/Xiaomi-YLDP03YL-Yeelight-Dimmable-Equivalent/dp/B077GCYCT7/), but the system is extensible to work with other bulbs. I didn't go with phillips hue because they require a hub and are more expensive.

### Sunroom

Where my computer is.

* If it's dark:
    * Walk in and a [motion detector](https://www.amazon.ca/ZOOZ-Z-Wave-Sensor-Temperature-Humidity/dp/B01AKSO80O) will trigger the lights to come on at a lower brightness than usual.
    * Waking up the computer will cause the lights to fully come on.
    * Sleeping the computer will cause the lights to dim. Once no motion has been detected in the room, the lights will turn off automatically at that point.
* If it's not dark:
    * The lights will stay off. Once it gets dark the lights turn on automatically if the computer is on.
* I can force the light to be on by using the light switch or by using the device's mobile app.

### TODO

* Switches - I'll be configuring smart switches sometime in the future, but for now I'm just using powered switches.

## Code Overview

- [`MainApp`](MainApp) - Central server/application for controlling the entire system. I'm currently running this on a Mac.
    - Controls light bulbs temperature, colour, and brightness.
    - Runs a server which devices can send their power status to.
        - Ex. The computer tells the server it's gone to sleep and the server will dim the light in that room until there hasn't been movement for X seconds. After X seconds it will turn off the light.)
- [`Communication/ClientApi`](Communication/ClientApi) - API for communicating with the `MainApp`.
- [`Communication/PowerStatus.Client`](Communication/PowerStatus.Client) - A script for a device to communicate its power status.
- [`Communication/PowerStatus.WindowsService`](Communication/PowerStatus.WindowsService) - A service for communicating a windows machine's power status. Read more about why it was necessary on its page.
- [`Communication/Server`](Communication/Server) - Used by the `MainApp` to establish a server.
- [`Core`](Core) - Used by the `MainApp` for setting everything up. Contains common classes such as light bulbs, sensors, utilities, etc.
- [`Plugins`](Plugins) - Implement device specific implementations for general `Core` interfaces (ex. `YeeLightBulb` implements `ILightBulb`)
- [`ZWave`](ZWave) - Communicates with a plugged in z-wave USB stick.

All libraries are .net standard. `MainApp` and `Communication/PowerStatus.Client` are .net core. `Communication/PowerStatus.WindowsService` is a .net framework application.
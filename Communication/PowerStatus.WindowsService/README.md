# Power Status - Windows Service

This service listens for power events of a windows computer (ex. computer sleep or resume) and lets the server know about it.

## Install

1. Copy the release build to a folder that won't change.
2. Install this as a windows service.
    ```bat
    SC CREATE "Home Device Control - Power Status" binpath="C:\HomeDeviceControl\HomeDeviceControl.Communication.PowerStatus.WindowsService.exe"
    ```
3. In the service's properties go to the "Log On" tab and check "Allow service to interact with desktop". This will provide the service a message loop so it can receive power events ([Read more](https://msdn.microsoft.com/en-us/library/microsoft.win32.systemevents(v=vs.110).aspx)).
4. Change the settings in `HomeDeviceControl.Communication.PowerStatus.WindowsService.exe.config` to specify the generated GUID for the device and the url to contact the [Server](../Server).

## Why? System events unreliable

Unfortunately listening for `Kernel-Power` system events (see `System` in [Event Viewer](https://en.wikipedia.org/wiki/Event_Viewer)) via the [Task Scheduler](https://en.wikipedia.org/wiki/Windows_Task_Scheduler) was not reliable.

I wanted to use this with the [Task Scheduler](https://en.wikipedia.org/wiki/Windows_Task_Scheduler) and fire the [Power Status Client](../PowerStatus.Client), but it wouldn't run the sleep event's triggered task (event ID `42`) until it would wake up from power.

So the solution here is to create a windows service that reports back the device's status.

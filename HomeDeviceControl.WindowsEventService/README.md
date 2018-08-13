# Device Controls - Windows Event Service

This service listens for power events of a windows computer (ex. computer sleep or resume) and lets the server know about it.

## Install

1. Copy the release build to a folder that won't change.
2. Install this as a windows service.
    ```bat
    SC CREATE "Home Device Control - Windows Event Service" binpath="C:\HomeDeviceControl\HomeDeviceControl.WindowsEventService.exe"
    ```
3. In the service's properties:
    1. Go to the "Log On" tab and check "Allow service to interact with desktop". This will provide the service a message loop so it can receive power events ([Read more](https://msdn.microsoft.com/en-us/library/microsoft.win32.systemevents(v=vs.110).aspx)).
    2. Provide the start parameters of a generated GUID for the device and the url to contact the [Server](../HomeDeviceControl.Communication.Server) (ex. `7d115c0c-6181-4965-bceb-449781ecd27a http://192.168.1.125:8084`).

## Why? System events unreliable

Unfortunately listening for `Kernel-Power` system events (see `System` in [Event Viewer](https://en.wikipedia.org/wiki/Event_Viewer)) via the [Task Scheduler](https://en.wikipedia.org/wiki/Windows_Task_Scheduler) was not reliable.

I wanted to use this with the [Task Scheduler](https://en.wikipedia.org/wiki/Windows_Task_Scheduler) and fire the [Power Status Client](../HomeDeviceControl.Communication.PowerStatus.Client), but it wouldn't run the sleep event's triggered task (event ID `42`) until it would wake up from power.

So the solution here is to create a windows service that reports back the device's status.

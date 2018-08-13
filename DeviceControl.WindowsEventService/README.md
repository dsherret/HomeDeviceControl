# Device Controls - Windows Event Service

Copy the release build to a folder that won't change, then install this as a windows service.

```bat
SC CREATE "Device Control - Windows Event Service" binpath="C:\DeviceControl\DeviceControl.WindowsEventService.exe"
```

## Why? System events unreliable

Unfortunately listening for `Kernel-Power` system events (see `System` in [Event Viewer](https://en.wikipedia.org/wiki/Event_Viewer)) via the [Task Scheduler](https://en.wikipedia.org/wiki/Windows_Task_Scheduler) was not reliable.

I wanted to use this with the [Task Scheduler](https://en.wikipedia.org/wiki/Windows_Task_Scheduler) and fire the [Power Status Client](../DeviceControl.Communication.PowerStatus.Client), but it wouldn't run the sleep event's triggered task (event ID `42`) until it would wake up from power.

So the solution here is to create a windows service that reports back the device's status.

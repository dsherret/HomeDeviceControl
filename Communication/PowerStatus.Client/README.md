# Power Status Client

Creates a file that can be run with arguments to report back a device's power status.

## Example Use

Provide the device's GUID and whether it's on or off.

```bash
dotnet HomeDeviceControl.Communication.PowerStatus.Client.dll 09E9E275-5C73-4FD0-B44B-D6890B176B75 true
```

## Windows

Use the [PowerStatus.WindowsService](../PowerStatus.WindowsService) on Windows.

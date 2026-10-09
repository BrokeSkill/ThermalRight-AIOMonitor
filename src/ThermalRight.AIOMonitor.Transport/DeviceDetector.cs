using ThermalRight.AIOMonitor.Core;
using ThermalRight.AIOMonitor.Transport.WinUsb;

namespace ThermalRight.AIOMonitor.Transport;

public sealed record DetectedDevice(UsbId Id, Wire Wire, string DevicePath);

public static class DeviceDetector
{
    public static List<DetectedDevice> Enumerate()
    {
        var found = new List<DetectedDevice>();
        foreach (var device in WinUsbEnumerator.Enumerate())
        {
            var wire = KnownDevices.Resolve(device.Id);
            if (wire is not null)
            {
                found.Add(new DetectedDevice(device.Id, wire.Value, device.Path));
            }
        }

        return found;
    }
}

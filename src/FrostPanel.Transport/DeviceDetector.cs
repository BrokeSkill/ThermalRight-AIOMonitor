using FrostPanel.Core;
using LibUsbDotNet;
using LibUsbDotNet.Main;

namespace FrostPanel.Transport;

public sealed record DetectedDevice(UsbId Id, Wire Wire, string DevicePath);

public static class DeviceDetector
{
    public static List<DetectedDevice> Enumerate()
    {
        var found = new List<DetectedDevice>();
        foreach (UsbRegistry registry in UsbDevice.AllDevices)
        {
            var id = new UsbId((ushort)registry.Vid, (ushort)registry.Pid);
            var wire = KnownDevices.Resolve(id);
            if (wire is not null)
            {
                found.Add(new DetectedDevice(id, wire.Value, registry.DevicePath ?? string.Empty));
            }
        }

        return found;
    }
}

using ThermalRight.AIOMonitor.Core;
using ThermalRight.AIOMonitor.Transport.WinUsb;

namespace ThermalRight.AIOMonitor.Transport;

public static class TransportFactory
{
    public static PanelDevice Open(DetectedDevice device)
    {
        var (_, _, writeEndpoint) = WireOps.Handshake(device.Wire);
        var transport = WinUsbTransport.Open(device.Id.Vid, device.Id.Pid, writeEndpoint);
        return new PanelDevice(device, transport);
    }
}

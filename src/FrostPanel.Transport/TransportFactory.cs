using FrostPanel.Core;

namespace FrostPanel.Transport;

public static class TransportFactory
{
    public static PanelDevice Open(DetectedDevice device)
    {
        var (_, _, writeEndpoint) = WireOps.Handshake(device.Wire);
        var transport = LibUsbTransport.Open(device.Id.Vid, device.Id.Pid, writeEndpoint);
        return new PanelDevice(device, transport);
    }
}

using FrostPanel.Core;
using FrostPanel.Transport.WinUsb;

namespace FrostPanel.Transport;

public static class TransportFactory
{
    public static PanelDevice Open(DetectedDevice device)
    {
        var (_, _, writeEndpoint) = WireOps.Handshake(device.Wire);
        var transport = WinUsbTransport.Open(device.Id.Vid, device.Id.Pid, writeEndpoint);
        return new PanelDevice(device, transport);
    }
}

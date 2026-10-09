using ThermalRight.AIOMonitor.Core;
using ThermalRight.AIOMonitor.Core.Framing;

namespace ThermalRight.AIOMonitor.Transport;

public sealed class PanelDevice : IDisposable
{
    private readonly IUsbTransport _transport;

    public DetectedDevice Device { get; }

    public HandshakeResult? Handshake { get; private set; }

    public PanelProfile Profile { get; private set; } = Profiles.Default;

    public PanelDevice(DetectedDevice device, IUsbTransport transport)
    {
        Device = device;
        _transport = transport;
    }

    public void Connect()
    {
        if (Device.Wire == Wire.Scsi)
        {
            throw new NotSupportedException("SCSI panels need the pass-through transport, which is not wired up yet");
        }

        var (request, readSize, _) = WireOps.Handshake(Device.Wire);
        _transport.Write(request, 2000);
        var response = _transport.Read(readSize, 2000);
        if (!WireOps.TryParseHandshake(Device.Wire, response, out var result))
        {
            throw new InvalidOperationException($"{Device.Wire} handshake rejected for {Device.Id}");
        }

        Handshake = result;
        Profile = Profiles.Get(result.Fbl, result.PmByte, result.SubByte);
    }

    public int SendFrame(byte[] payload)
    {
        if (Handshake is null)
        {
            throw new InvalidOperationException("connect first");
        }

        byte[] frame = Device.Wire switch
        {
            Wire.Bulk => BulkFraming.BuildFrame(payload, Profile),
            Wire.HidType2 => HidType2Framing.BuildFrame(payload, Profile),
            Wire.F5 => F5Framing.BuildFrame(payload, F5Framing.PayloadSize((byte)(Handshake.Fbl + 1))),
            Wire.Ly => LyFraming.BuildFrame(payload, Wire.Ly),
            Wire.Ly1 => LyFraming.BuildFrame(payload, Wire.Ly1),
            _ => throw new NotSupportedException(Device.Wire.ToString()),
        };

        return _transport.Write(frame, 5000);
    }

    public void Dispose() => _transport.Dispose();
}

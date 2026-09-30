using FrostPanel.Core.Framing;

namespace FrostPanel.Core;

public static class WireOps
{
    public static (byte[] Request, int ReadSize, byte WriteEndpoint) Handshake(Wire wire) => wire switch
    {
        Wire.Bulk => (BulkFraming.HandshakeRequest(), BulkFraming.HandshakeReadSize, BulkFraming.WriteEndpoint),
        Wire.HidType2 => (HidType2Framing.HandshakeRequest(), HidType2Framing.HandshakeReadSize, HidType2Framing.WriteEndpoint),
        Wire.F5 => (F5Framing.HandshakeRequest(), F5Framing.HandshakeReadSize, F5Framing.WriteEndpoint),
        Wire.Ly => (LyFraming.HandshakeRequest(), LyFraming.HandshakeReadSize, LyFraming.WriteEndpoint(Wire.Ly)),
        Wire.Ly1 => (LyFraming.HandshakeRequest(), LyFraming.HandshakeReadSize, LyFraming.WriteEndpoint(Wire.Ly1)),
        Wire.Scsi => (ScsiFraming.PollCdb(), ScsiFraming.PollSize, 0x00),
        _ => throw new ArgumentOutOfRangeException(nameof(wire), wire, null),
    };

    public static bool TryParseHandshake(Wire wire, byte[] response, out HandshakeResult result) => wire switch
    {
        Wire.Bulk => BulkFraming.TryParse(response, out result),
        Wire.HidType2 => HidType2Framing.TryParse(response, out result),
        Wire.F5 => F5Framing.TryParse(response, out result),
        Wire.Ly => LyFraming.TryParse(response, Wire.Ly, out result),
        Wire.Ly1 => LyFraming.TryParse(response, Wire.Ly1, out result),
        Wire.Scsi => ScsiFraming.TryParse(response, out result),
        _ => throw new ArgumentOutOfRangeException(nameof(wire), wire, null),
    };
}

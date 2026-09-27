using System.Buffers.Binary;

namespace FrostPanel.Core.Framing;

public static class BulkFraming
{
    public const byte WriteEndpoint = 0x01;
    public const byte ReadEndpoint = 0x81;
    public const int HandshakeReadSize = 1024;
    public const int WriteChunkSize = 16 * 1024;

    public static byte[] HandshakeRequest()
    {
        var request = new byte[64];
        request[0] = 0x12;
        request[1] = 0x34;
        request[2] = 0x56;
        request[3] = 0x78;
        request[56] = 0x01;
        return request;
    }

    public static bool TryParse(byte[] response, out HandshakeResult result)
    {
        result = null!;
        if (response.Length < 41 || response[24] == 0)
        {
            return false;
        }

        int pm = response[24];
        int sub = response[36];
        bool named = Profiles.InPmLadder(pm, sub);
        int fbl = named ? Profiles.PmToFbl(pm, sub) : 72;
        var profile = named ? Profiles.Get(fbl, pm, sub) : Profiles.Get(72, pm, sub);
        result = new HandshakeResult(profile.Width, profile.Height, pm, pm, sub, fbl, response);
        return true;
    }

    public static byte[] BuildFrame(byte[] payload, PanelProfile profile)
    {
        int cmd = profile.Jpeg ? 2 : 3;
        var frame = new byte[64 + payload.Length];
        frame[0] = 0x12;
        frame[1] = 0x34;
        frame[2] = 0x56;
        frame[3] = 0x78;
        BinaryPrimitives.WriteUInt32LittleEndian(frame.AsSpan(4), (uint)cmd);
        BinaryPrimitives.WriteUInt32LittleEndian(frame.AsSpan(8), (uint)profile.Width);
        BinaryPrimitives.WriteUInt32LittleEndian(frame.AsSpan(12), (uint)profile.Height);
        BinaryPrimitives.WriteUInt32LittleEndian(frame.AsSpan(56), 2);
        BinaryPrimitives.WriteUInt32LittleEndian(frame.AsSpan(60), (uint)payload.Length);
        payload.CopyTo(frame, 64);
        return frame;
    }
}

using System.Buffers.Binary;

namespace ThermalRight.AIOMonitor.Core.Framing;

public static class HidType2Framing
{
    public const byte WriteEndpoint = 0x02;
    public const byte ReadEndpoint = 0x81;
    public const int HandshakeReadSize = 512;
    public const int Alignment = 512;

    private static readonly byte[] Magic = { 0xDA, 0xDB, 0xDC, 0xDD };

    public static byte[] HandshakeRequest()
    {
        var request = new byte[512];
        Magic.CopyTo(request, 0);
        request[12] = 0x01;
        return request;
    }

    public static bool TryParse(byte[] response, out HandshakeResult result)
    {
        result = null!;
        if (response.Length < 20)
        {
            return false;
        }

        if (response[0] != 0xDA || response[1] != 0xDB || response[2] != 0xDC || response[3] != 0xDD)
        {
            return false;
        }

        if (response[12] != 0x01)
        {
            return false;
        }

        int pm = response[5];
        int sub = response[4];
        int fbl = Profiles.PmToFbl(pm, sub);
        var profile = Profiles.Get(fbl, pm, sub);
        result = new HandshakeResult(profile.Width, profile.Height, pm, pm, sub, fbl, response);
        return true;
    }

    public static byte[] BuildFrame(byte[] payload, PanelProfile profile)
    {
        var header = new byte[20];
        Magic.CopyTo(header, 0);
        header[4] = 0x02;
        if (profile.Jpeg)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(8), (ushort)profile.Width);
            BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(10), (ushort)profile.Height);
        }
        else
        {
            header[6] = 0x01;
            BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(8), 240);
            BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(10), 320);
        }

        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(16), (uint)payload.Length);

        int raw = header.Length + payload.Length;
        int aligned = (raw + Alignment - 1) / Alignment * Alignment;
        var frame = new byte[aligned];
        header.CopyTo(frame, 0);
        payload.CopyTo(frame, header.Length);
        return frame;
    }
}

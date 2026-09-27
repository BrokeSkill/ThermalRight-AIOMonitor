using System.Buffers.Binary;

namespace FrostPanel.Core.Framing;

public static class F5Framing
{
    public const byte WriteEndpoint = 0x02;
    public const byte ReadEndpoint = 0x81;
    public const int HandshakeReadSize = 1024;
    public const int AckSize = 16;
    public const int DataSize = 204800;
    public const int SmallDataSize = 153600;

    private static readonly byte[] CmdPrefix = { 0xF5, 0x00, 0x01, 0x00, 0xBC, 0xFF, 0xB6, 0xC8 };
    private static readonly byte[] FramePrefix = { 0xF5, 0x01, 0x01, 0x00, 0xBC, 0xFF, 0xB6, 0xC8 };

    public static bool IsValidIdentity(byte identity) => identity is 0x36 or 0x65 or 0x66;

    public static int PayloadSize(byte identity) => identity == 0x36 ? SmallDataSize : DataSize;

    public static byte[] HandshakeRequest()
    {
        var request = new byte[1040];
        CmdPrefix.CopyTo(request, 0);
        request[13] = 0x04;
        return request;
    }

    public static bool TryParse(byte[] response, out HandshakeResult result)
    {
        result = null!;
        if (response.Length < 14 || !IsValidIdentity(response[0]))
        {
            return false;
        }

        int fbl = response[0] - 1;
        var profile = Profiles.Get(fbl, fbl, 0);
        result = new HandshakeResult(profile.Width, profile.Height, fbl, fbl, 0, fbl, response);
        return true;
    }

    public static byte[] BuildFrame(byte[] payload, int payloadSize)
    {
        var frame = new byte[16 + payloadSize];
        FramePrefix.CopyTo(frame, 0);
        BinaryPrimitives.WriteUInt32LittleEndian(frame.AsSpan(12), (uint)payloadSize);
        Array.Copy(payload, 0, frame, 16, Math.Min(payload.Length, payloadSize));
        return frame;
    }
}

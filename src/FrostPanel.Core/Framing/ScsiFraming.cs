using System.Buffers.Binary;

namespace FrostPanel.Core.Framing;

public static class ScsiFraming
{
    public const int PollCmd = 0xF5;
    public const int InitCmd = 0x1F5;
    public const int PollSize = 0xE100;
    public const int FrameCmdBase = 0x101F5;
    public const int ChunkLarge = 0x10000;
    public const int ChunkSmall = 0xE100;
    public const int SmallDisplayPixels = 76800;

    public static byte[] BuildCdb(int cmd, int size)
    {
        var cdb = new byte[16];
        BinaryPrimitives.WriteInt32LittleEndian(cdb.AsSpan(0), cmd);
        BinaryPrimitives.WriteInt32LittleEndian(cdb.AsSpan(12), size);
        return cdb;
    }

    public static byte[] PollCdb() => BuildCdb(PollCmd, PollSize);

    public static byte[] InitCdb() => BuildCdb(InitCmd, PollSize);

    public static byte[] InitData() => new byte[PollSize];

    public static bool IsBooting(byte[] response) =>
        response.Length >= 8 &&
        response[4] == 0xA1 && response[5] == 0xA2 && response[6] == 0xA3 && response[7] == 0xA4;

    public static bool TryParse(byte[] response, out HandshakeResult result)
    {
        result = null!;
        if (response.Length < 1)
        {
            return false;
        }

        int fbl = response[0];
        var profile = Profiles.ForScsi(fbl);
        result = new HandshakeResult(profile.Width, profile.Height, fbl, fbl, 0, fbl, response);
        return true;
    }

    public static List<(int Cmd, int Size)> FrameChunks(int width, int height)
    {
        int pixels = width * height;
        int chunk = pixels <= SmallDisplayPixels ? ChunkSmall : ChunkLarge;
        int total = pixels * 2;
        var chunks = new List<(int, int)>();
        int offset = 0;
        int index = 0;
        while (offset < total)
        {
            int size = Math.Min(chunk, total - offset);
            chunks.Add((FrameCmdBase | (index << 24), size));
            offset += size;
            index++;
        }

        return chunks;
    }

    public static byte[] PadPayload(byte[] payload, int width, int height)
    {
        int total = width * height * 2;
        if (payload.Length >= total)
        {
            return payload;
        }

        var padded = new byte[total];
        payload.CopyTo(padded, 0);
        return padded;
    }
}

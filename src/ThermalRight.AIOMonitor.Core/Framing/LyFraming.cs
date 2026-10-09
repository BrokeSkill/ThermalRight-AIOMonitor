using System.Buffers.Binary;

namespace ThermalRight.AIOMonitor.Core.Framing;

public static class LyFraming
{
    public const byte ReadEndpoint = 0x81;
    public const int HandshakeReadSize = 512;
    public const int ChunkSize = 512;
    public const int ChunkHeaderSize = 16;
    public const int ChunkDataSize = 496;
    public const int UsbWriteSize = 4096;
    public const int AckReadSize = 512;

    private static readonly byte[] HandshakeHeader =
    {
        0x02, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    };

    public static byte WriteEndpoint(Wire wire) => wire == Wire.Ly ? (byte)0x09 : (byte)0x02;

    public static byte[] HandshakeRequest()
    {
        var request = new byte[2048];
        HandshakeHeader.CopyTo(request, 0);
        return request;
    }

    public static bool TryParse(byte[] response, Wire wire, out HandshakeResult result)
    {
        result = null!;
        if (response.Length < 37 || response[0] != 3 || response[1] != 0xFF || response[8] != 1)
        {
            return false;
        }

        int pm;
        int sub;
        if (wire == Wire.Ly)
        {
            int raw = response[20];
            if (raw <= 3)
            {
                raw = 1;
            }

            pm = 64 + raw;
            sub = response[22] + 1;
        }
        else
        {
            pm = 49 + response[20];
            sub = response[22];
        }

        bool named = Profiles.InPmLadder(pm, sub);
        int fbl = named ? Profiles.PmToFbl(pm, sub) : 72;
        var profile = named ? Profiles.Get(fbl, pm, sub) : Profiles.Get(72, pm, sub);
        result = new HandshakeResult(profile.Width, profile.Height, pm, pm, sub, fbl, response);
        return true;
    }

    public static byte[] BuildFrame(byte[] payload, Wire wire)
    {
        int total = payload.Length;
        int chunkCount = total / ChunkDataSize + 1;
        int lastData = total % ChunkDataSize;
        byte chunkCmd = wire == Wire.Ly ? (byte)1 : (byte)2;

        var chunks = new byte[chunkCount * ChunkSize];
        for (int i = 0; i < chunkCount; i++)
        {
            int offset = i * ChunkSize;
            int dataLen = i == chunkCount - 1 ? lastData : ChunkDataSize;
            chunks[offset] = 0x01;
            chunks[offset + 1] = 0xFF;
            BinaryPrimitives.WriteUInt32LittleEndian(chunks.AsSpan(offset + 2), (uint)total);
            BinaryPrimitives.WriteUInt16LittleEndian(chunks.AsSpan(offset + 6), (ushort)dataLen);
            chunks[offset + 8] = chunkCmd;
            BinaryPrimitives.WriteUInt16LittleEndian(chunks.AsSpan(offset + 9), (ushort)chunkCount);
            BinaryPrimitives.WriteUInt16LittleEndian(chunks.AsSpan(offset + 11), (ushort)i);
            Array.Copy(payload, i * ChunkDataSize, chunks, offset + ChunkHeaderSize, dataLen);
        }

        int multiple = wire == Wire.Ly ? 4 : 1;
        int padded = chunkCount;
        int remainder = padded % multiple;
        if (remainder != 0)
        {
            padded += multiple - remainder;
        }

        var frame = new byte[padded * ChunkSize];
        chunks.CopyTo(frame, 0);
        return frame;
    }
}

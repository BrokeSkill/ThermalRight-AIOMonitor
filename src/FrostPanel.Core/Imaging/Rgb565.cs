namespace FrostPanel.Core.Imaging;

public static class Rgb565
{
    public static ushort FromBgra(byte b, byte g, byte r) =>
        (ushort)(((r >> 3) << 11) | ((g >> 2) << 5) | (b >> 3));

    public static byte[] Encode(ReadOnlySpan<byte> bgra, bool bigEndian)
    {
        int pixels = bgra.Length / 4;
        var output = new byte[pixels * 2];
        for (int i = 0; i < pixels; i++)
        {
            int o = i * 4;
            ushort value = FromBgra(bgra[o], bgra[o + 1], bgra[o + 2]);
            if (bigEndian)
            {
                output[i * 2] = (byte)(value >> 8);
                output[i * 2 + 1] = (byte)value;
            }
            else
            {
                output[i * 2] = (byte)value;
                output[i * 2 + 1] = (byte)(value >> 8);
            }
        }

        return output;
    }
}

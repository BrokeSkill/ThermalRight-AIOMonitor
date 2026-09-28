namespace FrostPanel.Core.Imaging;

public static class BgraOps
{
    public const int BytesPerPixel = 4;

    public static byte[] Resample(byte[] source, int sourceWidth, int sourceHeight, int targetWidth, int targetHeight)
    {
        var target = new byte[targetWidth * targetHeight * BytesPerPixel];
        for (int y = 0; y < targetHeight; y++)
        {
            int sy = y * sourceHeight / targetHeight;
            for (int x = 0; x < targetWidth; x++)
            {
                int sx = x * sourceWidth / targetWidth;
                int s = (sy * sourceWidth + sx) * BytesPerPixel;
                int t = (y * targetWidth + x) * BytesPerPixel;
                target[t] = source[s];
                target[t + 1] = source[s + 1];
                target[t + 2] = source[s + 2];
                target[t + 3] = source[s + 3];
            }
        }

        return target;
    }

    public static (byte[] Data, int Width, int Height) Rotate(byte[] source, int width, int height, int degrees)
    {
        int angle = ((degrees % 360) + 360) % 360;
        if (angle == 0)
        {
            return (source, width, height);
        }

        if (angle == 180)
        {
            var flipped = new byte[source.Length];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int s = (y * width + x) * BytesPerPixel;
                    int t = ((height - 1 - y) * width + (width - 1 - x)) * BytesPerPixel;
                    Copy(source, s, flipped, t);
                }
            }

            return (flipped, width, height);
        }

        int newWidth = height;
        int newHeight = width;
        var rotated = new byte[source.Length];
        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                int sx = angle == 90 ? y : width - 1 - y;
                int sy = angle == 90 ? height - 1 - x : x;
                int s = (sy * width + sx) * BytesPerPixel;
                int t = (y * newWidth + x) * BytesPerPixel;
                Copy(source, s, rotated, t);
            }
        }

        return (rotated, newWidth, newHeight);
    }

    private static void Copy(byte[] source, int s, byte[] target, int t)
    {
        target[t] = source[s];
        target[t + 1] = source[s + 1];
        target[t + 2] = source[s + 2];
        target[t + 3] = source[s + 3];
    }
}

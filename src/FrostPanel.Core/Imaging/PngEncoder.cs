namespace FrostPanel.Core.Imaging;

public static class PngEncoder
{
    public static byte[] Encode(byte[] bgra, int width, int height)
    {
        using var bitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        var rect = new System.Drawing.Rectangle(0, 0, width, height);
        var locked = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);
        System.Runtime.InteropServices.Marshal.Copy(bgra, 0, locked.Scan0, bgra.Length);
        bitmap.UnlockBits(locked);

        using var stream = new MemoryStream();
        bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        return stream.ToArray();
    }
}

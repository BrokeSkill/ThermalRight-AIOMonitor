using System.Drawing;
using System.Runtime.InteropServices;

namespace FrostPanel.Capture;

public sealed record MonitorInfo(int Index, string DeviceName, int Left, int Top, int Width, int Height, bool Primary);

public sealed record BgraFrame(int Width, int Height, byte[] Data);

public static class MonitorCapture
{
    public static List<MonitorInfo> Enumerate()
    {
        var monitors = new List<MonitorInfo>();
        var screens = Screen.AllScreens;
        for (int i = 0; i < screens.Length; i++)
        {
            var screen = screens[i];
            monitors.Add(new MonitorInfo(
                i,
                screen.DeviceName,
                screen.Bounds.Left,
                screen.Bounds.Top,
                screen.Bounds.Width,
                screen.Bounds.Height,
                screen.Primary));
        }

        if (monitors.Count == 0)
        {
            monitors.Add(new MonitorInfo(0, @"\\.\DISPLAY1", 0, 0, 1, 1, true));
        }

        return monitors;
    }

    public static string FriendlyName(string deviceName) => deviceName;

    public static BgraFrame Capture(MonitorInfo monitor)
    {
        var bounds = new Rectangle(monitor.Left, monitor.Top, monitor.Width, monitor.Height);
        using var bitmap = new Bitmap(bounds.Width, bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var canvas = Graphics.FromImage(bitmap))
        {
            canvas.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bitmap.Size);
        }

        var locked = bitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            System.Drawing.Imaging.ImageLockMode.ReadOnly,
            System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        var data = new byte[bitmap.Width * bitmap.Height * 4];
        Marshal.Copy(locked.Scan0, data, 0, data.Length);
        bitmap.UnlockBits(locked);

        return new BgraFrame(bitmap.Width, bitmap.Height, data);
    }
}

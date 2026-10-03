using System.Runtime.InteropServices;

namespace FrostPanel.Capture;

public sealed record MonitorInfo(int Index, string DeviceName, int Left, int Top, int Width, int Height, bool Primary);

public sealed record BgraFrame(int Width, int Height, byte[] Data);

public static class MonitorCapture
{
    public static List<MonitorInfo> Enumerate()
    {
        var monitors = new List<MonitorInfo>();
        NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdc, ref Rect rect, IntPtr data) =>
        {
            var info = new MonitorInfoNative { cbSize = Marshal.SizeOf<MonitorInfoNative>() };
            if (NativeMethods.GetMonitorInfo(hMonitor, ref info))
            {
                var area = info.rcMonitor;
                monitors.Add(new MonitorInfo(
                    monitors.Count,
                    info.szDevice,
                    area.Left,
                    area.Top,
                    area.Right - area.Left,
                    area.Bottom - area.Top,
                    (info.dwFlags & NativeMethods.MonitorInfoPrimary) != 0));
            }

            return true;
        }, IntPtr.Zero);

        return monitors;
    }

    public static string FriendlyName(string deviceName)
    {
        var device = new DisplayDevice { cb = Marshal.SizeOf<DisplayDevice>() };
        return NativeMethods.EnumDisplayDevices(deviceName, 0, ref device, 0) ? device.DeviceString : string.Empty;
    }

    public static BgraFrame Capture(MonitorInfo monitor)
    {
        int width = monitor.Width;
        int height = monitor.Height;
        IntPtr screenDc = NativeMethods.CreateDC("DISPLAY", monitor.DeviceName, null, IntPtr.Zero);
        if (screenDc == IntPtr.Zero)
        {
            throw new InvalidOperationException($"no device context for {monitor.DeviceName}");
        }

        IntPtr memoryDc = NativeMethods.CreateCompatibleDC(screenDc);
        var info = new BitmapInfo
        {
            bmiHeader = new BitmapInfoHeader
            {
                biSize = Marshal.SizeOf<BitmapInfoHeader>(),
                biWidth = width,
                biHeight = -height,
                biPlanes = 1,
                biBitCount = 32,
            },
        };

        IntPtr bitmap = NativeMethods.CreateDIBSection(screenDc, ref info, 0, out IntPtr bits, IntPtr.Zero, 0);
        IntPtr previous = NativeMethods.SelectObject(memoryDc, bitmap);
        NativeMethods.BitBlt(memoryDc, 0, 0, width, height, screenDc, 0, 0, NativeMethods.SrcCopy);

        var data = new byte[width * height * 4];
        Marshal.Copy(bits, data, 0, data.Length);

        NativeMethods.SelectObject(memoryDc, previous);
        NativeMethods.DeleteObject(bitmap);
        NativeMethods.DeleteDC(memoryDc);
        NativeMethods.DeleteDC(screenDc);

        return new BgraFrame(width, height, data);
    }
}

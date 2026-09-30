using System.Runtime.InteropServices;

namespace FrostPanel.Capture;

internal static class NativeMethods
{
    internal const uint SrcCopy = 0x00CC0020;
    internal const uint MonitorInfoPrimary = 0x00000001;

    internal delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdc, ref Rect rect, IntPtr data);

    [DllImport("user32.dll")]
    internal static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr clip, MonitorEnumProc callback, IntPtr data);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoNative info);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
    internal static extern IntPtr CreateDC(string? driver, string? device, string? port, IntPtr devMode);

    [DllImport("gdi32.dll")]
    internal static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    internal static extern IntPtr CreateDIBSection(IntPtr hdc, ref BitmapInfo info, uint usage, out IntPtr bits, IntPtr section, uint offset);

    [DllImport("gdi32.dll")]
    internal static extern IntPtr SelectObject(IntPtr hdc, IntPtr obj);

    [DllImport("gdi32.dll")]
    internal static extern bool BitBlt(IntPtr dst, int x, int y, int width, int height, IntPtr src, int srcX, int srcY, uint rop);

    [DllImport("gdi32.dll")]
    internal static extern bool DeleteObject(IntPtr obj);

    [DllImport("gdi32.dll")]
    internal static extern bool DeleteDC(IntPtr hdc);
}

[StructLayout(LayoutKind.Sequential)]
internal struct Rect
{
    internal int Left;
    internal int Top;
    internal int Right;
    internal int Bottom;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct MonitorInfoNative
{
    internal int cbSize;
    internal Rect rcMonitor;
    internal Rect rcWork;
    internal uint dwFlags;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    internal string szDevice;
}

[StructLayout(LayoutKind.Sequential)]
internal struct BitmapInfoHeader
{
    internal int biSize;
    internal int biWidth;
    internal int biHeight;
    internal short biPlanes;
    internal short biBitCount;
    internal int biCompression;
    internal int biSizeImage;
    internal int biXPelsPerMeter;
    internal int biYPelsPerMeter;
    internal int biClrUsed;
    internal int biClrImportant;
}

[StructLayout(LayoutKind.Sequential)]
internal struct BitmapInfo
{
    internal BitmapInfoHeader bmiHeader;
    internal int bmiColors;
}

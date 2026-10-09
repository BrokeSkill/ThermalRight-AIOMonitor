using System.Runtime.InteropServices;

namespace FrostPanel.Transport.WinUsb;

internal static class WinUsbApi
{
    internal static readonly Guid UsbDeviceInterface = new("A5DCBF10-6530-11D2-901F-00C04FB951ED");
    internal static readonly IntPtr InvalidHandle = new(-1);

    internal const uint DigcfPresent = 0x02;
    internal const uint DigcfDeviceInterface = 0x10;
    internal const uint GenericRead = 0x80000000;
    internal const uint GenericWrite = 0x40000000;
    internal const uint FileShareRead = 0x01;
    internal const uint FileShareWrite = 0x02;
    internal const uint OpenExisting = 3;
    internal const uint FileFlagOverlapped = 0x40000000;
    internal const uint PipeTransferTimeout = 0x03;

    [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern IntPtr SetupDiGetClassDevs(ref Guid classGuid, IntPtr enumerator, IntPtr parent, uint flags);

    [DllImport("setupapi.dll", SetLastError = true)]
    internal static extern bool SetupDiEnumDeviceInterfaces(IntPtr set, IntPtr info, ref Guid classGuid, uint index, ref SpDeviceInterfaceData data);

    [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr set, ref SpDeviceInterfaceData data, IntPtr detail, uint detailSize, out uint required, IntPtr info);

    [DllImport("setupapi.dll", SetLastError = true)]
    internal static extern bool SetupDiDestroyDeviceInfoList(IntPtr set);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern IntPtr CreateFile(string fileName, uint access, uint share, IntPtr security, uint disposition, uint flags, IntPtr template);

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool CloseHandle(IntPtr handle);

    [DllImport("winusb.dll", SetLastError = true)]
    internal static extern bool WinUsb_Initialize(IntPtr deviceHandle, out IntPtr interfaceHandle);

    [DllImport("winusb.dll", SetLastError = true)]
    internal static extern bool WinUsb_Free(IntPtr interfaceHandle);

    [DllImport("winusb.dll", SetLastError = true)]
    internal static extern bool WinUsb_SetPipePolicy(IntPtr interfaceHandle, byte pipeId, uint policyType, uint valueLength, ref uint value);

    [DllImport("winusb.dll", SetLastError = true)]
    internal static extern bool WinUsb_WritePipe(IntPtr interfaceHandle, byte pipeId, IntPtr buffer, uint length, out uint transferred, IntPtr overlapped);

    [DllImport("winusb.dll", SetLastError = true)]
    internal static extern bool WinUsb_ReadPipe(IntPtr interfaceHandle, byte pipeId, IntPtr buffer, uint length, out uint transferred, IntPtr overlapped);

    [DllImport("winusb.dll", SetLastError = true)]
    internal static extern bool WinUsb_GetOverlappedResult(IntPtr interfaceHandle, IntPtr overlapped, out uint transferred, bool wait);

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern IntPtr CreateEvent(IntPtr attributes, bool manualReset, bool initialState, string? name);

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool ResetEvent(IntPtr handle);

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);
}

[StructLayout(LayoutKind.Sequential)]
internal struct SpDeviceInterfaceData
{
    internal int cbSize;
    internal Guid interfaceClassGuid;
    internal int flags;
    internal IntPtr reserved;
}

[StructLayout(LayoutKind.Sequential)]
internal struct Overlapped
{
    internal IntPtr Internal;
    internal IntPtr InternalHigh;
    internal uint Offset;
    internal uint OffsetHigh;
    internal IntPtr Event;
}

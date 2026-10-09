using System.Runtime.InteropServices;
using FrostPanel.Core;

namespace FrostPanel.Transport.WinUsb;

internal sealed record WinUsbDevice(UsbId Id, string Path);

internal static class WinUsbEnumerator
{
    internal static List<WinUsbDevice> Enumerate()
    {
        var found = new List<WinUsbDevice>();
        var guid = WinUsbApi.UsbDeviceInterface;
        IntPtr set = WinUsbApi.SetupDiGetClassDevs(ref guid, IntPtr.Zero, IntPtr.Zero, WinUsbApi.DigcfPresent | WinUsbApi.DigcfDeviceInterface);
        if (set == WinUsbApi.InvalidHandle)
        {
            return found;
        }

        try
        {
            var data = new SpDeviceInterfaceData { cbSize = Marshal.SizeOf<SpDeviceInterfaceData>() };
            for (uint index = 0; WinUsbApi.SetupDiEnumDeviceInterfaces(set, IntPtr.Zero, ref guid, index, ref data); index++)
            {
                WinUsbApi.SetupDiGetDeviceInterfaceDetail(set, ref data, IntPtr.Zero, 0, out uint required, IntPtr.Zero);
                if (required == 0)
                {
                    continue;
                }

                IntPtr buffer = Marshal.AllocHGlobal((int)required);
                try
                {
                    Marshal.WriteInt32(buffer, IntPtr.Size == 8 ? 8 : 6);
                    if (WinUsbApi.SetupDiGetDeviceInterfaceDetail(set, ref data, buffer, required, out _, IntPtr.Zero))
                    {
                        string path = Marshal.PtrToStringUni(buffer + 4) ?? string.Empty;
                        if (TryParseId(path, out var id))
                        {
                            found.Add(new WinUsbDevice(id, path));
                        }
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
        }
        finally
        {
            WinUsbApi.SetupDiDestroyDeviceInfoList(set);
        }

        return found;
    }

    private static bool TryParseId(string path, out UsbId id)
    {
        id = default;
        int vid = path.IndexOf("vid_", StringComparison.OrdinalIgnoreCase);
        int pid = path.IndexOf("pid_", StringComparison.OrdinalIgnoreCase);
        if (vid < 0 || pid < 0 || vid + 8 > path.Length || pid + 8 > path.Length)
        {
            return false;
        }

        if (!ushort.TryParse(path.AsSpan(vid + 4, 4), System.Globalization.NumberStyles.HexNumber, null, out ushort vendor) ||
            !ushort.TryParse(path.AsSpan(pid + 4, 4), System.Globalization.NumberStyles.HexNumber, null, out ushort product))
        {
            return false;
        }

        id = new UsbId(vendor, product);
        return true;
    }
}

public sealed class WinUsbTransport : IUsbTransport
{
    private readonly IntPtr _file;
    private readonly IntPtr _interface;
    private readonly IntPtr _event;
    private readonly byte _writeEndpoint;
    private bool _disposed;

    private WinUsbTransport(IntPtr file, IntPtr interfaceHandle, IntPtr completionEvent, byte writeEndpoint)
    {
        _file = file;
        _interface = interfaceHandle;
        _event = completionEvent;
        _writeEndpoint = writeEndpoint;
    }

    public static WinUsbTransport Open(int vid, int pid, byte writeEndpoint)
    {
        var match = WinUsbEnumerator.Enumerate().Find(d => d.Id.Vid == vid && d.Id.Pid == pid)
            ?? throw new InvalidOperationException($"usb device {vid:x4}:{pid:x4} not found");

        IntPtr file = WinUsbApi.CreateFile(
            match.Path,
            WinUsbApi.GenericRead | WinUsbApi.GenericWrite,
            WinUsbApi.FileShareRead | WinUsbApi.FileShareWrite,
            IntPtr.Zero,
            WinUsbApi.OpenExisting,
            WinUsbApi.FileFlagOverlapped,
            IntPtr.Zero);
        if (file == WinUsbApi.InvalidHandle)
        {
            int error = Marshal.GetLastWin32Error();
            string hint = error == 5
                ? " - the device is in use by another program; close TRCC or other Thermalright software, and make sure only one copy of FrostPanel is running, then retry"
                : string.Empty;
            throw new InvalidOperationException($"open failed for {match.Path} (win32 {error}){hint}");
        }

        if (!WinUsbApi.WinUsb_Initialize(file, out IntPtr interfaceHandle))
        {
            int error = Marshal.GetLastWin32Error();
            WinUsbApi.CloseHandle(file);
            throw new InvalidOperationException($"WinUsb_Initialize failed (win32 {error})");
        }

        return new WinUsbTransport(file, interfaceHandle, WinUsbApi.CreateEvent(IntPtr.Zero, true, false, null), writeEndpoint);
    }

    public int Write(byte[] data, int timeoutMs)
    {
        SetTimeout(_writeEndpoint, timeoutMs);
        var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        try
        {
            int sent = Transfer(_writeEndpoint, handle.AddrOfPinnedObject(), data.Length, true, timeoutMs);
            if (data.Length > 0 && data.Length % 512 == 0)
            {
                Transfer(_writeEndpoint, handle.AddrOfPinnedObject(), 0, true, timeoutMs);
            }

            return sent;
        }
        finally
        {
            handle.Free();
        }
    }

    public byte[] Read(int size, int timeoutMs)
    {
        SetTimeout(0x81, timeoutMs);
        var buffer = new byte[size];
        var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        try
        {
            int got = Transfer(0x81, handle.AddrOfPinnedObject(), size, false, timeoutMs);
            return got == size ? buffer : buffer[..got];
        }
        finally
        {
            handle.Free();
        }
    }

    private int Transfer(byte pipeId, IntPtr buffer, int length, bool isWrite, int timeoutMs)
    {
        IntPtr overlapped = Marshal.AllocHGlobal(Marshal.SizeOf<Overlapped>());
        try
        {
            int eventOffset = IntPtr.Size == 8 ? 24 : 16;
            Marshal.WriteIntPtr(overlapped, 0, IntPtr.Zero);
            Marshal.WriteIntPtr(overlapped, IntPtr.Size, IntPtr.Zero);
            Marshal.WriteInt32(overlapped, IntPtr.Size * 2, 0);
            Marshal.WriteInt32(overlapped, IntPtr.Size * 2 + 4, 0);
            Marshal.WriteIntPtr(overlapped, eventOffset, _event);
            WinUsbApi.ResetEvent(_event);

            uint transferred;
            bool ok;
            if (isWrite)
            {
                ok = WinUsbApi.WinUsb_WritePipe(_interface, pipeId, buffer, (uint)length, out transferred, overlapped);
            }
            else
            {
                ok = WinUsbApi.WinUsb_ReadPipe(_interface, pipeId, buffer, (uint)length, out transferred, overlapped);
            }

            string verb = isWrite ? "Write" : "Read";
            if (!ok)
            {
                int error = Marshal.GetLastWin32Error();
                if (error != 997)
                {
                    throw new IOException($"WinUsb_{verb}Pipe failed (win32 {error})");
                }

                if (WinUsbApi.WaitForSingleObject(_event, (uint)timeoutMs) != 0)
                {
                    throw new IOException($"WinUsb_{verb}Pipe timed out - the panel stopped responding; unplug and replug its USB cable to reset it");
                }

                if (!WinUsbApi.WinUsb_GetOverlappedResult(_interface, overlapped, out transferred, false))
                {
                    throw new IOException($"WinUsb_GetOverlappedResult failed (win32 {Marshal.GetLastWin32Error()})");
                }
            }

            return (int)transferred;
        }
        finally
        {
            Marshal.FreeHGlobal(overlapped);
        }
    }

    private void SetTimeout(byte pipeId, int timeoutMs)
    {
        uint value = (uint)timeoutMs;
        WinUsbApi.WinUsb_SetPipePolicy(_interface, pipeId, WinUsbApi.PipeTransferTimeout, sizeof(uint), ref value);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        WinUsbApi.WinUsb_Free(_interface);
        WinUsbApi.CloseHandle(_file);
        WinUsbApi.CloseHandle(_event);
    }
}

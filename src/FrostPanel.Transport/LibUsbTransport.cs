using LibUsbDotNet;
using LibUsbDotNet.Main;

namespace FrostPanel.Transport;

public sealed class LibUsbTransport : IUsbTransport
{
    private readonly UsbDevice _device;
    private readonly UsbEndpointWriter _writer;
    private readonly UsbEndpointReader _reader;

    private LibUsbTransport(UsbDevice device, byte writeEndpoint)
    {
        _device = device;
        _writer = device.OpenEndpointWriter(ToWriteEndpoint(writeEndpoint));
        _reader = device.OpenEndpointReader(ReadEndpointID.Ep01);
    }

    public static LibUsbTransport Open(int vid, int pid, byte writeEndpoint)
    {
        var device = UsbDevice.OpenUsbDevice(new UsbDeviceFinder(vid, pid))
            ?? throw new InvalidOperationException($"usb device {vid:x4}:{pid:x4} not found");
        device.Open();
        return new LibUsbTransport(device, writeEndpoint);
    }

    public int Write(byte[] data, int timeoutMs)
    {
        _writer.Write(data, timeoutMs, out int transferred);
        return transferred;
    }

    public byte[] Read(int size, int timeoutMs)
    {
        var buffer = new byte[size];
        _reader.Read(buffer, timeoutMs, out int transferred);
        return transferred == size ? buffer : buffer[..transferred];
    }

    public void Dispose()
    {
        _writer.Dispose();
        _reader.Dispose();
        if (_device.IsOpen)
        {
            _device.Close();
        }
    }

    private static WriteEndpointID ToWriteEndpoint(byte endpoint) => endpoint switch
    {
        0x01 => WriteEndpointID.Ep01,
        0x02 => WriteEndpointID.Ep02,
        0x03 => WriteEndpointID.Ep03,
        0x04 => WriteEndpointID.Ep04,
        0x05 => WriteEndpointID.Ep05,
        0x06 => WriteEndpointID.Ep06,
        0x07 => WriteEndpointID.Ep07,
        0x08 => WriteEndpointID.Ep08,
        0x09 => WriteEndpointID.Ep09,
        _ => throw new ArgumentOutOfRangeException(nameof(endpoint), endpoint, "unsupported write endpoint"),
    };
}

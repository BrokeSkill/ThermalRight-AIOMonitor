namespace ThermalRight.AIOMonitor.Transport;

public interface IUsbTransport : IDisposable
{
    int Write(byte[] data, int timeoutMs);

    byte[] Read(int size, int timeoutMs);
}

using FrostPanel.Capture;
using FrostPanel.Core;
using FrostPanel.Core.Imaging;
using FrostPanel.Transport;

namespace FrostPanel.App;

public sealed class PanelPipeline : IDisposable
{
    private readonly PanelDevice _device;
    private readonly MonitorInfo _monitor;
    private readonly int _rotationDegrees;
    private readonly FramePacer _pacer;

    public PanelPipeline(PanelDevice device, MonitorInfo monitor, int rotationDegrees, int maxFps)
    {
        _device = device;
        _monitor = monitor;
        _rotationDegrees = rotationDegrees;
        _pacer = new FramePacer(maxFps);
    }

    public long FramesSent { get; private set; }

    public PanelProfile Profile => _device.Profile;

    public bool RenderOnce(double nowMs)
    {
        if (!_pacer.TryAcquire(nowMs))
        {
            return false;
        }

        var frame = MonitorCapture.Capture(_monitor);
        var profile = _device.Profile;
        var payload = FrameConverter.ToPanelPayload(
            frame.Data,
            frame.Width,
            frame.Height,
            profile.Width,
            profile.Height,
            _rotationDegrees,
            profile.BigEndian);
        _device.SendFrame(payload);
        FramesSent++;
        return true;
    }

    public void Dispose() => _device.Dispose();
}

using ThermalRight.AIOMonitor.Capture;
using ThermalRight.AIOMonitor.Core;
using ThermalRight.AIOMonitor.Core.Imaging;
using ThermalRight.AIOMonitor.Transport;

namespace ThermalRight.AIOMonitor.App;

public sealed class PanelPipeline : IDisposable
{
    private readonly PanelDevice _device;
    private readonly MonitorInfo _monitor;
    private readonly int _rotationDegrees;
    private readonly FramePacer _pacer;
    private readonly int _jpegQuality;

    public PanelPipeline(PanelDevice device, MonitorInfo monitor, int rotationDegrees, int maxFps, int jpegQuality = 90)
    {
        _device = device;
        _monitor = monitor;
        _rotationDegrees = rotationDegrees;
        _pacer = new FramePacer(maxFps);
        _jpegQuality = jpegQuality;
    }

    public long FramesSent { get; private set; }

    public PanelProfile Profile => _device.Profile;

    public byte[]? LastPng { get; private set; }

    public bool RenderOnce(double nowMs)
    {
        if (!_pacer.TryAcquire(nowMs))
        {
            return false;
        }

        var profile = _device.Profile;
        var frame = MonitorCapture.Capture(_monitor);
        var scaled = ThermalRight.AIOMonitor.Core.Imaging.BgraOps.Resample(
            frame.Data,
            frame.Width,
            frame.Height,
            profile.Width,
            profile.Height);
        LastPng = ThermalRight.AIOMonitor.Core.Imaging.PngEncoder.Encode(scaled, profile.Width, profile.Height);
        var payload = profile.Jpeg
            ? FrameConverter.ToJpegPayload(
                frame.Data,
                frame.Width,
                frame.Height,
                profile.Width,
                profile.Height,
                _rotationDegrees,
                _jpegQuality)
            : FrameConverter.ToPanelPayload(
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

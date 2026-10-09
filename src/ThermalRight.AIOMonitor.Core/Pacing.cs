namespace ThermalRight.AIOMonitor.Core;

public sealed class FramePacer
{
    private readonly double _minIntervalMs;
    private double _lastMs = double.NegativeInfinity;

    public FramePacer(int maxFps)
    {
        _minIntervalMs = maxFps <= 0 ? 0 : 1000.0 / maxFps;
    }

    public bool TryAcquire(double nowMs)
    {
        if (nowMs - _lastMs < _minIntervalMs)
        {
            return false;
        }

        _lastMs = nowMs;
        return true;
    }
}

public static class Throughput
{
    public static int FpsFor(double bytesPerSecond, int frameBytes)
    {
        if (bytesPerSecond <= 0 || frameBytes <= 0)
        {
            return 0;
        }

        return (int)Math.Max(1, bytesPerSecond / frameBytes);
    }
}

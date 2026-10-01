using System.Diagnostics;
using FrostPanel.App;
using FrostPanel.Capture;
using FrostPanel.Transport;

var devices = DeviceDetector.Enumerate();
if (devices.Count == 0)
{
    Console.WriteLine("no Thermalright panel found");
    return 1;
}

foreach (var candidate in devices)
{
    Console.WriteLine($"device {candidate.Id} wire {candidate.Wire} path {candidate.DevicePath}");
}

var monitors = MonitorCapture.Enumerate();
if (monitors.Count == 0)
{
    Console.WriteLine("no monitors found");
    return 1;
}

foreach (var monitor in monitors)
{
    Console.WriteLine($"monitor {monitor.Index} {monitor.DeviceName} {monitor.Width}x{monitor.Height}{(monitor.Primary ? " primary" : string.Empty)}");
}

int monitorIndex = args.Length > 0 && int.TryParse(args[0], out var parsed) ? parsed : 0;
if (monitorIndex < 0 || monitorIndex >= monitors.Count)
{
    Console.WriteLine($"monitor index {monitorIndex} out of range");
    return 1;
}

int maxFps = args.Length > 1 && int.TryParse(args[1], out var fps) ? fps : 15;

var selected = devices[0];
using var panel = TransportFactory.Open(selected);
panel.Connect();
Console.WriteLine($"panel {panel.Profile.Width}x{panel.Profile.Height} jpeg={panel.Profile.Jpeg} bigEndian={panel.Profile.BigEndian}");

using var pipeline = new PanelPipeline(panel, monitors[monitorIndex], 0, maxFps);
var clock = Stopwatch.StartNew();
double lastReport = 0;

while (true)
{
    if (!pipeline.RenderOnce(clock.Elapsed.TotalMilliseconds))
    {
        Thread.Sleep(1);
        continue;
    }

    double seconds = clock.Elapsed.TotalSeconds;
    if (seconds - lastReport >= 1)
    {
        lastReport = seconds;
        Console.WriteLine($"{pipeline.FramesSent} frames, {pipeline.FramesSent / seconds:F1} fps");
    }
}

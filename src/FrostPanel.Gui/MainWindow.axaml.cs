using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using FrostPanel.App;
using FrostPanel.Capture;
using FrostPanel.Transport;

namespace FrostPanel.Gui;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(15) };
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly ListBox _deviceList;
    private readonly ComboBox _monitorBox;
    private readonly ComboBox _rotationBox;
    private readonly Slider _fpsSlider;
    private readonly TextBlock _modelText;
    private readonly TextBlock _panelText;
    private readonly TextBlock _encodingText;
    private readonly TextBlock _fpsText;
    private readonly TextBlock _statusText;
    private readonly Button _startButton;
    private readonly Button _stopButton;

    private List<DetectedDevice> _devices = new();
    private List<MonitorInfo> _monitors = new();
    private PanelDevice? _panel;
    private PanelPipeline? _pipeline;
    private double _lastReport;
    private long _lastFrames;

    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        _deviceList = this.FindControl<ListBox>("DeviceList")!;
        _monitorBox = this.FindControl<ComboBox>("MonitorBox")!;
        _rotationBox = this.FindControl<ComboBox>("RotationBox")!;
        _fpsSlider = this.FindControl<Slider>("FpsSlider")!;
        _modelText = this.FindControl<TextBlock>("ModelText")!;
        _panelText = this.FindControl<TextBlock>("PanelText")!;
        _encodingText = this.FindControl<TextBlock>("EncodingText")!;
        _fpsText = this.FindControl<TextBlock>("FpsText")!;
        _statusText = this.FindControl<TextBlock>("StatusText")!;
        _startButton = this.FindControl<Button>("StartButton")!;
        _stopButton = this.FindControl<Button>("StopButton")!;
        var rescanButton = this.FindControl<Button>("RescanButton")!;

        _rotationBox.ItemsSource = new[] { "0", "90", "180", "270" };
        _rotationBox.SelectedIndex = 0;
        rescanButton.Click += (_, _) => Rescan();
        _startButton.Click += (_, _) => Start();
        _stopButton.Click += (_, _) => Stop();
        _deviceList.SelectionChanged += (_, _) => ShowSelected();
        _timer.Tick += (_, _) => Tick();
        Rescan();
    }

    private void Rescan()
    {
        _devices = DeviceDetector.Enumerate();
        _monitors = MonitorCapture.Enumerate();
        _deviceList.ItemsSource = _devices.Select(device => $"{device.Id}  {device.Wire}").ToList();
        _monitorBox.ItemsSource = _monitors.Select(monitor => $"{monitor.Index}: {monitor.DeviceName} {monitor.Width}x{monitor.Height}").ToList();
        if (_monitors.Count > 0)
        {
            _monitorBox.SelectedIndex = 0;
        }

        if (_devices.Count > 0)
        {
            _deviceList.SelectedIndex = 0;
        }

        _statusText.Text = _devices.Count == 0 ? "No panel found" : "Ready";
    }

    private void ShowSelected()
    {
        int index = _deviceList.SelectedIndex;
        if (index < 0 || index >= _devices.Count)
        {
            return;
        }

        var device = _devices[index];
        _modelText.Text = device.Id.ToString();
        _statusText.Text = $"Selected {device.Id} on the {device.Wire} wire";
    }

    private void Start()
    {
        if (_devices.Count == 0 || _monitors.Count == 0)
        {
            return;
        }

        int deviceIndex = Math.Max(0, _deviceList.SelectedIndex);
        int monitorIndex = Math.Max(0, _monitorBox.SelectedIndex);
        int rotation = Math.Max(0, _rotationBox.SelectedIndex) * 90;
        int fps = (int)_fpsSlider.Value;

        try
        {
            _panel = TransportFactory.Open(_devices[deviceIndex]);
            _panel.Connect();
            _pipeline = new PanelPipeline(_panel, _monitors[monitorIndex], rotation, fps);
            var profile = _panel.Profile;
            _panelText.Text = $"{profile.Width}x{profile.Height}";
            _encodingText.Text = profile.Jpeg ? "JPEG" : profile.BigEndian ? "RGB565 BE" : "RGB565 LE";
            _lastReport = _clock.Elapsed.TotalSeconds;
            _lastFrames = 0;
            _timer.Start();
            _startButton.IsEnabled = false;
            _stopButton.IsEnabled = true;
            _statusText.Text = "Streaming";
        }
        catch (Exception error)
        {
            _statusText.Text = error.Message;
            Stop();
        }
    }

    private void Stop()
    {
        _timer.Stop();
        _pipeline?.Dispose();
        _pipeline = null;
        _panel?.Dispose();
        _panel = null;
        _startButton.IsEnabled = true;
        _stopButton.IsEnabled = false;
        _fpsText.Text = "0.0";
    }

    private void Tick()
    {
        if (_pipeline is null)
        {
            return;
        }

        try
        {
            _pipeline.RenderOnce(_clock.Elapsed.TotalMilliseconds);
        }
        catch (Exception error)
        {
            _statusText.Text = error.Message;
            Stop();
            return;
        }

        double seconds = _clock.Elapsed.TotalSeconds;
        if (seconds - _lastReport >= 1)
        {
            long frames = _pipeline.FramesSent;
            _fpsText.Text = ((frames - _lastFrames) / (seconds - _lastReport)).ToString("F1");
            _lastFrames = frames;
            _lastReport = seconds;
        }
    }
}

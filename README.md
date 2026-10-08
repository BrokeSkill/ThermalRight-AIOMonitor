# FrostPanel

Turns a Thermalright USB AIO screen into a real second monitor on Windows.

Windows sees a new display. You drag windows onto it. Whatever lands there shows
up on the cooler's LCD. It is an extended desktop, not a wallpaper overlay.

## How it works

The panel has no video input. It is a USB serial device that draws whatever
frames you send it. So FrostPanel builds a monitor in software, lets Windows
render to it, copies those pixels, and pushes them down the USB protocol the
panel already speaks.

```
Virtual Display Driver  ->  Windows renders the desktop to a real monitor
        |
   screen grab (GDI)  ->  BGRA pixels of that monitor
        |
   convert  ->  scale, rotate, RGB565
        |
   USB  ->  the panel's own wire protocol
```

Four pieces:

- `FrostPanel.Core` the wire protocols, the panel profile tables, pixel
  conversion and frame pacing. Plain C#, no Windows calls.
- `FrostPanel.Transport` device detection and USB I/O through LibUsbDotNet,
  the same library Thermalright's own Windows tool uses.
- `FrostPanel.Capture` display enumeration and a monitor grab.
- `FrostPanel.App` the pipeline that ties a panel to a monitor.
- `FrostPanel.Gui` the desktop app. `FrostPanel.Cli` a headless driver.

## Requirements

- Windows 10 or 11, x64.
- The Virtual Display Driver, for the virtual monitor. The installer adds it.
- .NET 8 SDK, to build.
- A supported panel (see below).

## Build

```
dotnet build
dotnet publish src/FrostPanel.Gui -c Release -r win-x64 -o artifacts/publish
```

It runs anywhere .NET runs.

## Install

Elevated PowerShell:

```
installer\install.ps1 -Width 480 -Height 480
```

It publishes nothing itself; build first. The script installs the driver,
writes the driver settings for your panel's resolution, copies the app to
Program Files, and registers it at login. Reboot once so the driver loads.

To remove it:

```
installer\uninstall.ps1
```

## Use

The app lists the panels it found, the displays you can capture, and lets you
set a rotation and an fps cap. Pick a panel, pick the monitor, press Start. The
live fps readout tells you what the panel actually sustains.

Headless:

```
frostpanel [monitorIndex] [maxFps]
```

## Supported panels

Detection is by USB id, then by the handshake the panel answers with, so the
model and resolution come from the device itself rather than the id alone.

| USB id     | Wire     | Typical panel |
|------------|----------|---------------|
| 87CD:70DB  | SCSI     | 320x320 |
| 87AD:70DB  | Bulk     | 480x480 (GrandVision, Mjolnir, Hyper Vision, Peerless Vision) |
| 0402:3922  | SCSI     | 320x320 (Frozen Warframe) |
| 0416:5406  | F5       | 320x320 (LC series) |
| 0416:5302  | HID type 2 | 240x240 to 1280x480 (Trofeo Vision, Frozen Warframe SE) |
| 0418:5303  | F5       | TARAN ARMS |
| 0418:5304  | F5       | TARAN ARMS |
| 0416:5408  | LY       | Trofeo Vision 9.16 |
| 0416:5409  | LY1      | Trofeo Vision 9.16 |

## Limitations

These are real and current.

- JPEG panels are not wired yet. Many of the wide panels (1280x480, 1920x462,
  854x480, 360x360) take a JPEG frame. FrostPanel only encodes RGB565, so it
  refuses those panels with a clear error instead of painting garbage. The
  RGB565 panels (320x320, 480x480, the 320x240 family) work.
- SCSI panels are stubbed. The framing is written, the SCSI pass-through
  transport is not.
- Capture is GDI BitBlt, not DXGI duplication. Simpler and dependency-free, and
  fast enough for a panel that rarely clears 30 fps. Swap it if you see tearing
  or protected content.
- Refresh is low by nature. A 480x480 frame is 460 KB. Full-speed USB gives a
  few frames a second, high-speed more, but the panel's own chip usually caps
  it. This is a glanceable second screen, not a gaming display.
- Not yet exercised on real hardware. It builds clean and the wire formats are
  pinned to the source, but the first run on glass is still ahead.

## License

GPL-3.0. See `LICENSE`.

The protocol knowledge comes from the trcc-linux project, which is GPL-3.0 and
decompiled Thermalright's own Windows binary. That is why FrostPanel is GPL too.

Not affiliated with or endorsed by Thermalright. "Thermalright" is their
trademark and appears here only to say what the software is for.

## Credits

- trcc-linux (Lexonight1) for the reverse-engineered USB protocols and the
  device tables. https://github.com/Lexonight1/thermalright-trcc-linux
- Virtual Display Driver (VirtualDrivers) for the virtual monitor. MIT.
  https://github.com/VirtualDrivers/Virtual-Display-Driver
- LibUsbDotNet for the USB layer.

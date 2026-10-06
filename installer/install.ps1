param(
    [int]$Width = 480,
    [int]$Height = 480,
    [int]$Refresh = 60,
    [string]$Source = "$PSScriptRoot\..\artifacts\publish"
)

$ErrorActionPreference = "Stop"

$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object Security.Principal.WindowsPrincipal($identity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw "Run this from an elevated PowerShell."
}

if (-not (Test-Path $Source)) {
    throw "Publish output not found at $Source. Run: dotnet publish src/FrostPanel.Gui -c Release -r win-x64 -o artifacts/publish"
}

$target = Join-Path $env:ProgramFiles "FrostPanel"
New-Item -ItemType Directory -Force -Path $target | Out-Null
Copy-Item -Path (Join-Path $Source "*") -Destination $target -Recurse -Force

if (Get-Command winget -ErrorAction SilentlyContinue) {
    winget install --id VirtualDrivers.Virtual-Display-Driver -e --accept-source-agreements --accept-package-agreements
}
else {
    Write-Warning "winget not found. Install the Virtual Display Driver manually: https://github.com/VirtualDrivers/Virtual-Display-Driver"
}

$configDir = "C:\VirtualDisplayDriver"
New-Item -ItemType Directory -Force -Path $configDir | Out-Null
$xml = @"
<?xml version='1.0' encoding='utf-8'?>
<vdd_settings>
  <monitors><count>1</count></monitors>
  <gpu><friendlyname>default</friendlyname></gpu>
  <global><g_refresh_rate>$Refresh</g_refresh_rate></global>
  <resolutions><resolution><width>$Width</width><height>$Height</height><refresh_rate>$Refresh</refresh_rate></resolution></resolutions>
  <edid><CustomEdid>false</CustomEdid><PreventSpoof>false</PreventSpoof><EdidCeaOverride>false</EdidCeaOverride></edid>
</vdd_settings>
"@
Set-Content -Path (Join-Path $configDir "vdd_settings.xml") -Value $xml -Encoding UTF8

$exe = Join-Path $target "FrostPanel.exe"
Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "FrostPanel" -Value $exe

Write-Host "FrostPanel installed to $target"
Write-Host "Virtual display configured at ${Width}x${Height} @ ${Refresh}Hz."
Write-Host "Reboot once so the driver loads, then start FrostPanel from the Start menu."

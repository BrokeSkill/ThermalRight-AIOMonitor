$ErrorActionPreference = "Stop"

Remove-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "FrostPanel" -ErrorAction SilentlyContinue
Remove-Item -Path (Join-Path $env:ProgramFiles "FrostPanel") -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "C:\VirtualDisplayDriver\vdd_settings.xml" -Force -ErrorAction SilentlyContinue

Write-Host "FrostPanel removed."
Write-Host "The Virtual Display Driver was left in place. Remove it with:"
Write-Host "  winget uninstall VirtualDrivers.Virtual-Display-Driver"

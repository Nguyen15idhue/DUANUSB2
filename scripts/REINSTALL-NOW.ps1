# Quick reinstall script for testing
Write-Host "=== REINSTALLING SERVICE ===" -ForegroundColor Cyan

# 1. Uninstall
Write-Host "1. Uninstalling..." -ForegroundColor Yellow
& "C:\Program Files\CHC Geomatics\Dongle Service\DongleSyncService.exe" uninstall
Start-Sleep 2

# 2. Copy new files
Write-Host "2. Copying new files..." -ForegroundColor Yellow
$source = "F:\3.Laptrinh\DUANUSB2\src\DongleSyncService\bin\Release\net8.0\win-x64\publish\*"
$dest = "C:\Program Files\CHC Geomatics\Dongle Service\"
Copy-Item $source -Destination $dest -Force -Recurse

# 3. Install
Write-Host "3. Installing..." -ForegroundColor Yellow
& "C:\Program Files\CHC Geomatics\Dongle Service\DongleSyncService.exe" install start
Start-Sleep 3

# 4. Verify
Write-Host "4. Verifying..." -ForegroundColor Green
Get-Service DongleSyncService | Format-List Status, DisplayName
Get-Item "C:\Program Files\CHC Geomatics\Dongle Service\DongleSyncService.dll" | Format-List LastWriteTime

Write-Host "`nDONE! Now unplug/replug USB to test." -ForegroundColor Cyan

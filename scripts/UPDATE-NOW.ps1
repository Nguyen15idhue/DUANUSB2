# UPDATE-NOW.ps1 - Quick update service files
Write-Host "Stopping service..." -ForegroundColor Yellow
Stop-Service DongleSyncService -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 3

Write-Host "Copying new files..." -ForegroundColor Yellow
$source = "F:\3.Laptrinh\DUANUSB2\src\DongleSyncService\bin\Release\net8.0\win-x64\publish"
$dest = "C:\Program Files\CHC Geomatics\Dongle Service"

Copy-Item "$source\*.dll" -Destination $dest -Force -Verbose
Copy-Item "$source\DongleSyncService.exe" -Destination $dest -Force -Verbose

Write-Host "Starting service..." -ForegroundColor Yellow
Start-Service DongleSyncService

Write-Host "Done! Service status:" -ForegroundColor Green
Get-Service DongleSyncService | Select-Object Status, Name

Write-Host "`nPress any key to exit..." -ForegroundColor Cyan
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

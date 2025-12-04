# This script must run as Administrator
# Updates service files while service is stopped

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Updating Service Files" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$source = "F:\3.Laptrinh\DUANUSB2\src\DongleSyncService\bin\Release\net8.0\win-x64\publish"
$dest = "C:\Program Files\CHC Geomatics\Dongle Service"

# Check if running as admin
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "ERROR: This script must run as Administrator" -ForegroundColor Red
    Write-Host "Right-click and select 'Run as Administrator'" -ForegroundColor Yellow
    exit 1
}

# Stop service
Write-Host "Stopping service..." -ForegroundColor Yellow
try {
    Stop-Service DongleSyncService -Force -ErrorAction Stop
    Write-Host "✅ Service stopped" -ForegroundColor Green
} catch {
    Write-Host "⚠ Failed to stop service: $($_.Exception.Message)" -ForegroundColor Yellow
}

Start-Sleep 2

# Copy files
Write-Host ""
Write-Host "Copying files from:" -ForegroundColor Cyan
Write-Host "  $source" -ForegroundColor Gray
Write-Host "To:" -ForegroundColor Cyan
Write-Host "  $dest" -ForegroundColor Gray
Write-Host ""

try {
    Copy-Item "$source\*" $dest -Recurse -Force
    Write-Host "✅ Files copied successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to copy files: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Start service
Write-Host ""
Write-Host "Starting service..." -ForegroundColor Yellow
try {
    Start-Service DongleSyncService -ErrorAction Stop
    Start-Sleep 2
    $service = Get-Service DongleSyncService
    Write-Host "✅ Service started: $($service.Status)" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to start service: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Verify file timestamp
Write-Host ""
$serviceExe = Get-Item "$dest\DongleSyncService.exe"
$publishExe = Get-Item "$source\DongleSyncService.exe"
Write-Host "Verification:" -ForegroundColor Cyan
Write-Host "  Service EXE: $($serviceExe.LastWriteTime)" -ForegroundColor Gray
Write-Host "  Source  EXE: $($publishExe.LastWriteTime)" -ForegroundColor Gray

if ($serviceExe.LastWriteTime -eq $publishExe.LastWriteTime) {
    Write-Host "✅ Update successful!" -ForegroundColor Green
} else {
    Write-Host "⚠ Warning: Timestamps don't match" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Done!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

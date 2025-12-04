# PowerShell Script: Reset Test Environment
# Purpose: Clean state for fresh testing

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Resetting Test Environment" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "ERROR: This script requires Administrator privileges" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please run PowerShell as Administrator and try again" -ForegroundColor Yellow
    exit 1
}

$DataDir = "C:\ProgramData\DongleSyncService"

# Step 1: Stop service
Write-Host "Step 1: Stopping service..." -ForegroundColor Yellow
$service = Get-Service -Name "DongleSyncService" -ErrorAction SilentlyContinue

if ($null -ne $service -and $service.Status -eq "Running") {
    Stop-Service -Name "DongleSyncService" -Force
    Write-Host "[OK] Service stopped" -ForegroundColor Green
} else {
    Write-Host "[OK] Service already stopped" -ForegroundColor Green
}
Write-Host ""

# Step 2: Delete synced DLL
Write-Host "Step 2: Removing synced DLL..." -ForegroundColor Yellow
$dllPath = "$DataDir\synced_patch.dll"
if (Test-Path $dllPath) {
    Remove-Item $dllPath -Force
    Write-Host "[OK] Synced DLL deleted" -ForegroundColor Green
} else {
    Write-Host "[OK] No synced DLL found" -ForegroundColor Green
}
Write-Host ""

# Step 3: Delete machine binding (optional - ask user)
Write-Host "Step 3: Machine binding..." -ForegroundColor Yellow
$bindKeyPath = "$DataDir\bind.key"
if (Test-Path $bindKeyPath) {
    $response = Read-Host "Delete bind.key to allow rebinding? (y/N)"
    if ($response -eq "y" -or $response -eq "Y") {
        Remove-Item $bindKeyPath -Force
        Write-Host "[OK] Machine binding cleared" -ForegroundColor Green
    } else {
        Write-Host "[SKIP] Machine binding kept" -ForegroundColor Yellow
    }
} else {
    Write-Host "[OK] No machine binding found" -ForegroundColor Green
}
Write-Host ""

# Step 4: Show current state
Write-Host "Step 4: Current state..." -ForegroundColor Yellow
Write-Host "  Service: " -NoNewline
if ($null -ne $service) {
    $service.Refresh()
    Write-Host $service.Status -ForegroundColor Cyan
} else {
    Write-Host "Not installed" -ForegroundColor Gray
}

Write-Host "  Synced DLL: " -NoNewline
if (Test-Path $dllPath) {
    Write-Host "EXISTS" -ForegroundColor Red
} else {
    Write-Host "Not found" -ForegroundColor Green
}

Write-Host "  Machine Binding: " -NoNewline
if (Test-Path $bindKeyPath) {
    Write-Host "BOUND" -ForegroundColor Cyan
} else {
    Write-Host "Not bound" -ForegroundColor Green
}
Write-Host ""

# Step 5: Start service (optional)
$response = Read-Host "Start service now? (Y/n)"
if ($response -ne "n" -and $response -ne "N") {
    Write-Host ""
    Write-Host "Starting service..." -ForegroundColor Yellow
    Start-Service -Name "DongleSyncService"
    Write-Host "[OK] Service started" -ForegroundColor Green
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Reset Complete - Ready for Testing" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "You can now:" -ForegroundColor Cyan
Write-Host "  1. Plug in USB dongle to test fresh sync" -ForegroundColor White
Write-Host "  2. Check logs: $DataDir\logs\" -ForegroundColor White
Write-Host ""

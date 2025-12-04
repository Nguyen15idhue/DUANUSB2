# PowerShell Script: Stop DongleSyncService
# Purpose: Stop the service for testing

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Stopping DongleSyncService" -ForegroundColor Cyan
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

# Check if service exists
$service = Get-Service -Name "DongleSyncService" -ErrorAction SilentlyContinue

if ($null -eq $service) {
    Write-Host "Service 'DongleSyncService' not found" -ForegroundColor Yellow
    Write-Host "Service may not be installed" -ForegroundColor Gray
    exit 0
}

# Show current status
Write-Host "Current Status: $($service.Status)" -ForegroundColor Cyan
Write-Host ""

# Stop the service
if ($service.Status -eq "Running") {
    Write-Host "Stopping service..." -ForegroundColor Yellow
    
    try {
        Stop-Service -Name "DongleSyncService" -Force -ErrorAction Stop
        Write-Host "[OK] Service stopped successfully" -ForegroundColor Green
    } catch {
        Write-Host "ERROR: Failed to stop service" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "[OK] Service is already stopped" -ForegroundColor Green
}

Write-Host ""
Write-Host "Service Status: Stopped" -ForegroundColor Green
Write-Host ""

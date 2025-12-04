# PowerShell Script: Restart DongleSyncService
# Purpose: Restart the service (stop + start)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Restarting DongleSyncService" -ForegroundColor Cyan
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
    Write-Host "ERROR: Service 'DongleSyncService' not found" -ForegroundColor Red
    Write-Host "Service may not be installed" -ForegroundColor Yellow
    exit 1
}

# Show current status
Write-Host "Current Status: $($service.Status)" -ForegroundColor Cyan
Write-Host ""

# Restart the service
Write-Host "Restarting service..." -ForegroundColor Yellow

try {
    Restart-Service -Name "DongleSyncService" -Force -ErrorAction Stop
    Write-Host "[OK] Service restarted successfully" -ForegroundColor Green
} catch {
    Write-Host "ERROR: Failed to restart service" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Check event log for details:" -ForegroundColor Yellow
    Write-Host "  Get-EventLog -LogName Application -Source DongleSyncService -Newest 5" -ForegroundColor Gray
    exit 1
}

Write-Host ""
Write-Host "Service Status: Running" -ForegroundColor Green
Write-Host ""

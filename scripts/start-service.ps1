# PowerShell Script: Start DongleSyncService
# Purpose: Start the service

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Starting DongleSyncService" -ForegroundColor Cyan
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

# Start the service
if ($service.Status -ne "Running") {
    Write-Host "Starting service..." -ForegroundColor Yellow
    
    try {
        Start-Service -Name "DongleSyncService" -ErrorAction Stop
        Write-Host "[OK] Service started successfully" -ForegroundColor Green
    } catch {
        Write-Host "ERROR: Failed to start service" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        Write-Host ""
        Write-Host "Check event log for details:" -ForegroundColor Yellow
        Write-Host "  Get-EventLog -LogName Application -Source DongleSyncService -Newest 5" -ForegroundColor Gray
        exit 1
    }
} else {
    Write-Host "[OK] Service is already running" -ForegroundColor Green
}

Write-Host ""
Write-Host "Service Status: Running" -ForegroundColor Green
Write-Host ""

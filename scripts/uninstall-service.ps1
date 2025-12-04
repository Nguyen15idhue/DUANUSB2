# ============================================
# UNINSTALL DONGLE SYNC SERVICE
# Run as Administrator
# ============================================

# Check Administrator
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "❌ ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  DONGLE SYNC SERVICE UNINSTALLER" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$serviceName = "DongleSyncService"

# Step 1: Check if service exists
$service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue

if (-not $service) {
    Write-Host "ℹ️  Service not found. Nothing to uninstall." -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 0
}

# Step 2: Stop service
Write-Host "⏹️  Stopping service..." -ForegroundColor Cyan
Stop-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 3

# Step 3: Remove service
Write-Host "🗑️  Removing service..." -ForegroundColor Cyan
sc.exe delete $serviceName | Out-Null
Start-Sleep -Seconds 2

# Step 4: Verify removal
$service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue

if ($service) {
    Write-Host "❌ Failed to remove service!" -ForegroundColor Red
    Write-Host "Try stopping the service manually and run this script again." -ForegroundColor Yellow
} else {
    Write-Host ""
    Write-Host "✅ Service uninstalled successfully!" -ForegroundColor Green
    Write-Host ""
    
    # Ask about data cleanup
    $cleanup = Read-Host "Do you want to delete service data? (y/n)"
    
    if ($cleanup -eq 'y' -or $cleanup -eq 'Y') {
        $dataDir = "C:\ProgramData\DongleSyncService"
        
        Write-Host "🗑️  Deleting service data..." -ForegroundColor Yellow
        
        if (Test-Path $dataDir) {
            Remove-Item -Path $dataDir -Recurse -Force -ErrorAction SilentlyContinue
            Write-Host "   Deleted: $dataDir" -ForegroundColor Gray
        }
        
        Write-Host "✅ Cleanup complete!" -ForegroundColor Green
    } else {
        Write-Host "ℹ️  Service data preserved at: C:\ProgramData\DongleSyncService" -ForegroundColor Cyan
    }
}

Write-Host ""
Write-Host "Press Enter to exit..."
Read-Host

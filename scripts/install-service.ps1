# ============================================
# INSTALL DONGLE SYNC SERVICE
# Run as Administrator
# ============================================

param(
    [string]$BinaryPath = "F:\3.Laptrinh\DUANUSB2\src\DongleSyncService\bin\Release\net8.0\DongleSyncService.exe"
)

# Check Administrator
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "❌ ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  DONGLE SYNC SERVICE INSTALLER" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Check if binary exists
if (-not (Test-Path $BinaryPath)) {
    Write-Host "❌ ERROR: Service binary not found!" -ForegroundColor Red
    Write-Host "Expected path: $BinaryPath" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Please build the project first:" -ForegroundColor Yellow
    Write-Host "  cd F:\3.Laptrinh\DUANUSB2\src" -ForegroundColor White
    Write-Host "  dotnet build -c Release" -ForegroundColor White
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "✅ Binary found: $BinaryPath" -ForegroundColor Green

# Step 2: Stop existing service
$serviceName = "DongleSyncService"
$existingService = Get-Service -Name $serviceName -ErrorAction SilentlyContinue

if ($existingService) {
    Write-Host "⚠️  Service already exists. Stopping..." -ForegroundColor Yellow
    Stop-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
    
    # Remove old service
    Write-Host "🗑️  Removing old service..." -ForegroundColor Yellow
    sc.exe delete $serviceName | Out-Null
    Start-Sleep -Seconds 2
}

# Step 3: Create service directories
$dataDir = "C:\ProgramData\DongleSyncService"
$backupDir = "$dataDir\backups"
$logDir = "$dataDir\logs"

Write-Host "📁 Creating service directories..." -ForegroundColor Cyan
@($dataDir, $backupDir, $logDir) | ForEach-Object {
    if (-not (Test-Path $_)) {
        New-Item -ItemType Directory -Path $_ -Force | Out-Null
        Write-Host "   Created: $_" -ForegroundColor Gray
    }
}

# Step 4: Create service
Write-Host "🔧 Installing Windows Service..." -ForegroundColor Cyan

$result = sc.exe create $serviceName `
    binPath= $BinaryPath `
    start= auto `
    DisplayName= "USB Dongle Sync Service" `
    obj= "LocalSystem"

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to create service!" -ForegroundColor Red
    Write-Host $result -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Step 5: Configure service description
sc.exe description $serviceName "Manages USB dongle authentication and DLL patching for CHC Geomatics Office 2" | Out-Null

# Step 6: Configure service recovery
Write-Host "⚙️  Configuring service recovery..." -ForegroundColor Cyan
sc.exe failure $serviceName reset= 86400 actions= restart/60000/restart/60000/restart/60000 | Out-Null

# Step 7: Start service
Write-Host "▶️  Starting service..." -ForegroundColor Cyan
Start-Service -Name $serviceName -ErrorAction SilentlyContinue
Start-Sleep -Seconds 3

# Step 8: Verify service status
$service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue

if ($service -and $service.Status -eq "Running") {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  ✅ SERVICE INSTALLED SUCCESSFULLY!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Service Name:    $serviceName" -ForegroundColor White
    Write-Host "Display Name:    USB Dongle Sync Service" -ForegroundColor White
    Write-Host "Status:          Running" -ForegroundColor Green
    Write-Host "Startup Type:    Automatic" -ForegroundColor White
    Write-Host "Account:         LocalSystem" -ForegroundColor White
    Write-Host "Binary Path:     $BinaryPath" -ForegroundColor Gray
    Write-Host "Data Directory:  $dataDir" -ForegroundColor Gray
    Write-Host ""
    Write-Host "📝 Logs location: $logDir" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "To view service status:" -ForegroundColor Yellow
    Write-Host "  Get-Service DongleSyncService" -ForegroundColor White
    Write-Host ""
    Write-Host "To view logs:" -ForegroundColor Yellow
    Write-Host "  Get-Content C:\ProgramData\DongleSyncService\logs\*.log -Tail 50" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "⚠️  Service installed but not running!" -ForegroundColor Yellow
    Write-Host "Check Windows Event Viewer for errors." -ForegroundColor Yellow
    Write-Host ""
}

Write-Host "Press Enter to exit..."
Read-Host

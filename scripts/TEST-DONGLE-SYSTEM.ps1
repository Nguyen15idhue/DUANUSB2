# =============================================
# Test Dongle System End-to-End
# =============================================

$ErrorActionPreference = "Continue"

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  DONGLE SYSTEM TEST" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Check USB Drives
Write-Host "[1] Checking USB Drives..." -ForegroundColor Yellow
$usbDrives = Get-WmiObject Win32_Volume | Where-Object {$_.DriveType -eq 2 -and $_.DriveLetter} | Select-Object -ExpandProperty DriveLetter
if ($usbDrives) {
    Write-Host "   ✓ USB Drives found: $($usbDrives -join ', ')" -ForegroundColor Green
    $targetDrive = $usbDrives[0]
} else {
    Write-Host "   ✗ No USB drives detected! Please insert USB." -ForegroundColor Red
    exit 1
}

# Step 2: Check dongle_config.json
Write-Host "`n[2] Checking dongle configuration..." -ForegroundColor Yellow
$configPath = Join-Path $targetDrive "dongle_config.json"
if (Test-Path $configPath) {
    Write-Host "   ✓ Config found: $configPath" -ForegroundColor Green
    $config = Get-Content $configPath | ConvertFrom-Json
    Write-Host "   USB GUID: $($config.usb_guid)" -ForegroundColor Gray
    Write-Host "   Features: $($config.features -join ', ')" -ForegroundColor Gray
    $hasConfig = $true
} else {
    Write-Host "   ✗ Config NOT found: $configPath" -ForegroundColor Red
    Write-Host "   → You need to create dongle using DongleCreatorTool" -ForegroundColor Yellow
    $hasConfig = $false
}

# Step 3: Check patch.dll.enc
Write-Host "`n[3] Checking encrypted patch DLL..." -ForegroundColor Yellow
$patchPath = Join-Path $targetDrive "patch.dll.enc"
if (Test-Path $patchPath) {
    $patchInfo = Get-Item $patchPath
    Write-Host "   ✓ Patch DLL found: $patchPath" -ForegroundColor Green
    Write-Host "   Size: $($patchInfo.Length) bytes" -ForegroundColor Gray
} else {
    Write-Host "   ✗ Patch DLL NOT found: $patchPath" -ForegroundColor Red
    Write-Host "   → You need to create dongle using DongleCreatorTool" -ForegroundColor Yellow
}

# Step 4: Check Service Status
Write-Host "`n[4] Checking DongleSyncService..." -ForegroundColor Yellow
$service = Get-Service DongleSyncService -ErrorAction SilentlyContinue
if ($service) {
    Write-Host "   ✓ Service installed" -ForegroundColor Green
    Write-Host "   Status: $($service.Status)" -ForegroundColor $(if ($service.Status -eq 'Running') {'Green'} else {'Red'})
} else {
    Write-Host "   ✗ Service NOT installed" -ForegroundColor Red
    exit 1
}

# Step 5: Check Service State
Write-Host "`n[5] Checking service state..." -ForegroundColor Yellow
$statePath = "C:\ProgramData\DongleSyncService\state.json"
if (Test-Path $statePath) {
    $state = Get-Content $statePath | ConvertFrom-Json
    Write-Host "   State file found" -ForegroundColor Green
    Write-Host "   Is Patched: $($state.isPatched)" -ForegroundColor $(if ($state.isPatched) {'Green'} else {'Gray'})
    Write-Host "   USB GUID: $($state.usbGuid)" -ForegroundColor Gray
    Write-Host "   DLL Path: $($state.dllPath)" -ForegroundColor Gray
    Write-Host "   Last Patch: $($state.lastPatchTime)" -ForegroundColor Gray
} else {
    Write-Host "   ✗ State file not found" -ForegroundColor Red
}

# Step 6: Check Target DLL
Write-Host "`n[6] Checking target application DLL..." -ForegroundColor Yellow
$targetDll = "C:\Users\ADMIN\AppData\Roaming\CHCNAV\CHC Geomatics Office 2\CHC.CGO.Common.dll"
if (Test-Path $targetDll) {
    $dllInfo = Get-Item $targetDll
    Write-Host "   ✓ Target DLL found: $targetDll" -ForegroundColor Green
    Write-Host "   Size: $($dllInfo.Length) bytes" -ForegroundColor Gray
    Write-Host "   Modified: $($dllInfo.LastWriteTime)" -ForegroundColor Gray
    
    # Check if DLL is in use
    try {
        $stream = [System.IO.File]::Open($targetDll, 'Open', 'ReadWrite', 'None')
        $stream.Close()
        Write-Host "   Status: Not in use (ready to patch)" -ForegroundColor Green
    } catch {
        Write-Host "   Status: In use by application" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ✗ Target DLL not found" -ForegroundColor Red
    Write-Host "   → CHC Geomatics Office may not be installed" -ForegroundColor Yellow
}

# Step 7: Check Recent Logs
Write-Host "`n[7] Checking recent service logs..." -ForegroundColor Yellow
$logPath = "C:\ProgramData\DongleSyncService\logs"
if (Test-Path $logPath) {
    $latestLog = Get-ChildItem $logPath -Filter "*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if ($latestLog) {
        Write-Host "   Latest log: $($latestLog.Name)" -ForegroundColor Green
        Write-Host "`n   Last 10 lines:" -ForegroundColor Gray
        Write-Host "   " + ("-" * 60) -ForegroundColor DarkGray
        Get-Content $latestLog.FullName -Tail 10 | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
        Write-Host "   " + ("-" * 60) -ForegroundColor DarkGray
    }
}

# Summary
Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  SUMMARY" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

if (-not $hasConfig) {
    Write-Host "[!] USB DONGLE NOT CONFIGURED!" -ForegroundColor Red
    Write-Host ""
    Write-Host "To create a dongle:" -ForegroundColor Yellow
    Write-Host "1. Run: F:\3.Laptrinh\DUANUSB2\output\DongleCreatorTool\DongleCreatorTool.exe" -ForegroundColor White
    Write-Host "2. Select USB drive: $targetDrive" -ForegroundColor White
    Write-Host "3. Select patch DLL: F:\3.Laptrinh\DUANUSB2\dllgoc\CHC.CGO.Common.dll" -ForegroundColor White
    Write-Host "4. Enable features and click Create Dongle" -ForegroundColor White
    Write-Host ""
    Write-Host "After creating dongle:" -ForegroundColor Yellow
    Write-Host "- Unplug and re-plug USB" -ForegroundColor White
    Write-Host "- Service will auto-detect and patch DLL" -ForegroundColor White
} else {
    Write-Host "[OK] Dongle is configured!" -ForegroundColor Green
    if ($service.Status -eq 'Running') {
        Write-Host "[OK] Service is running!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Next steps:" -ForegroundColor Yellow
        Write-Host "1. Unplug and re-plug USB to trigger service" -ForegroundColor White
        $logFile = Join-Path $logPath "service-$(Get-Date -Format 'yyyyMMdd').log"
        Write-Host "2. Check logs: Get-Content '$logFile' -Tail 30" -ForegroundColor White
        Write-Host "3. Launch CHC Geomatics Office to test features" -ForegroundColor White
    } else {
        Write-Host "[!] Service is NOT running!" -ForegroundColor Red
        Write-Host "Run: Start-Service DongleSyncService" -ForegroundColor White
    }
}

Write-Host ""

Write-Host ""

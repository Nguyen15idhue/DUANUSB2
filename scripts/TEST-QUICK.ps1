# Test Dongle System
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " DONGLE SYSTEM TEST" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. Check USB
Write-Host "[1] USB Drives..." -ForegroundColor Yellow
$usb = Get-WmiObject Win32_Volume | Where-Object {$_.DriveType -eq 2 -and $_.DriveLetter} | Select-Object -ExpandProperty DriveLetter
Write-Host "   Found: $($usb -join ', ')" -ForegroundColor Green

# 2. Check Config
Write-Host "[2] Dongle Config..." -ForegroundColor Yellow
if ($usb) {
    $cfg = Join-Path $usb[0] "dongle_config.json"
    if (Test-Path $cfg) {
        Write-Host "   FOUND: $cfg" -ForegroundColor Green
        Get-Content $cfg | ConvertFrom-Json | ConvertTo-Json
    } else {
        Write-Host "   NOT FOUND: $cfg" -ForegroundColor Red
        Write-Host "   >> Run DongleCreatorTool to create dongle!" -ForegroundColor Yellow
    }
}

# 3. Check Service
Write-Host "[3] Service Status..." -ForegroundColor Yellow
$svc = Get-Service DongleSyncService -ErrorAction SilentlyContinue
if ($svc) {
    Write-Host "   Status: $($svc.Status)" -ForegroundColor $(if ($svc.Status -eq 'Running') {'Green'} else {'Red'})
}

# 4. Check State
Write-Host "[4] Service State..." -ForegroundColor Yellow
$state = "C:\ProgramData\DongleSyncService\state.json"
if (Test-Path $state) {
    Get-Content $state | ConvertFrom-Json | ConvertTo-Json
}

Write-Host ""

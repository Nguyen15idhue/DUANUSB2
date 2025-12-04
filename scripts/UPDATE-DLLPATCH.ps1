# ===========================================
# Update DLLPatch.dll in Installed Service
# ===========================================

$ErrorActionPreference = "Stop"

# Check admin
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]"Administrator")) {
    Write-Host "ERROR: This script requires Administrator privileges" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    exit 1
}

$ServiceName = "DongleSyncService"
$InstallPath = "C:\Program Files\CHC Geomatics\Dongle Service"
$PublishPath = "F:\3.Laptrinh\DUANUSB2\src\DongleSyncService\bin\Release\net8.0\win-x64\publish"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Updating DLLPatch.dll" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Step 1: Stop Service
Write-Host "Step 1: Stopping service..." -NoNewline
try {
    $service = Get-Service $ServiceName -ErrorAction SilentlyContinue
    if ($service -and $service.Status -eq 'Running') {
        Stop-Service $ServiceName -Force
        Start-Sleep -Seconds 3
        Write-Host " [OK]" -ForegroundColor Green
    } else {
        Write-Host " [Already stopped]" -ForegroundColor Yellow
    }
} catch {
    Write-Host " [FAILED]" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 2: Copy DLLPatch.dll
Write-Host "Step 2: Copying DLLPatch.dll..." -NoNewline
try {
    $sourceDll = Join-Path $PublishPath "DLLPatch.dll"
    if (-not (Test-Path $sourceDll)) {
        throw "DLLPatch.dll not found in publish folder. Please run: dotnet publish -c Release"
    }
    
    Copy-Item $sourceDll -Destination $InstallPath -Force
    
    # Verify
    $destDll = Join-Path $InstallPath "DLLPatch.dll"
    if (Test-Path $destDll) {
        $fileInfo = Get-Item $destDll
        Write-Host " [OK]" -ForegroundColor Green
        Write-Host "   File: $($fileInfo.Length) bytes, Modified: $($fileInfo.LastWriteTime)" -ForegroundColor Gray
    } else {
        throw "Copy failed - file not found after copy"
    }
} catch {
    Write-Host " [FAILED]" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 3: Start Service
Write-Host "Step 3: Starting service..." -NoNewline
try {
    Start-Service $ServiceName
    Start-Sleep -Seconds 2
    
    $service = Get-Service $ServiceName
    if ($service.Status -eq 'Running') {
        Write-Host " [OK]" -ForegroundColor Green
    } else {
        Write-Host " [WARNING: Status = $($service.Status)]" -ForegroundColor Yellow
    }
} catch {
    Write-Host " [FAILED]" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "  UPDATE COMPLETED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Green

Write-Host "Service Status:" -ForegroundColor Cyan
Get-Service $ServiceName | Format-Table -AutoSize

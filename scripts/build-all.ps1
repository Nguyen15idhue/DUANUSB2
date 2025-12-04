# PowerShell Script: Build Complete Production Package
# Purpose: Run all build steps in sequence to create deployment package

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "USB Dongle Sync System - Build All" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "This script will execute:" -ForegroundColor Yellow
Write-Host "  1. Build DongleSyncService (Release)" -ForegroundColor Gray
Write-Host "  2. Publish DongleCreatorTool (Standalone)" -ForegroundColor Gray
Write-Host "  3. Build MSI Installer" -ForegroundColor Gray
Write-Host "  4. Create Deployment Package" -ForegroundColor Gray
Write-Host ""

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir
$ErrorOccurred = $false

# Step 1: Build Service
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "STEP 1/4: Building DongleSyncService" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

try {
    Push-Location "$ProjectRoot\src\DongleSyncService"
    $buildProcess = Start-Process -FilePath "dotnet" -ArgumentList "build", "-c", "Release" -NoNewWindow -Wait -PassThru
    Pop-Location
    
    if ($buildProcess.ExitCode -ne 0) {
        Write-Host "ERROR: Service build failed" -ForegroundColor Red
        $ErrorOccurred = $true
    } else {
        Write-Host "[OK] Service build successful" -ForegroundColor Green
    }
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Pop-Location
    $ErrorOccurred = $true
}

Write-Host ""

if ($ErrorOccurred) {
    Write-Host "Build process stopped due to errors" -ForegroundColor Red
    exit 1
}

# Step 2: Publish DongleCreatorTool
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "STEP 2/4: Publishing DongleCreatorTool" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

try {
    & "$ScriptDir\publish-dongle-creator.ps1"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: DongleCreatorTool publish failed" -ForegroundColor Red
        $ErrorOccurred = $true
    }
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    $ErrorOccurred = $true
}

Write-Host ""

if ($ErrorOccurred) {
    Write-Host "Build process stopped due to errors" -ForegroundColor Red
    exit 1
}

# Step 3: Build MSI
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "STEP 3/4: Building MSI Installer" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

try {
    & "$ScriptDir\build-msi.ps1"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: MSI build failed" -ForegroundColor Red
        $ErrorOccurred = $true
    }
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    $ErrorOccurred = $true
}

Write-Host ""

if ($ErrorOccurred) {
    Write-Host "Build process stopped due to errors" -ForegroundColor Red
    exit 1
}

# Step 4: Create Deployment Package
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "STEP 4/4: Creating Deployment Package" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

try {
    & "$ScriptDir\create-deployment-package.ps1"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Deployment package creation failed" -ForegroundColor Red
        $ErrorOccurred = $true
    }
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    $ErrorOccurred = $true
}

Write-Host ""

# Final Summary
if (-not $ErrorOccurred) {
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "[SUCCESS] ALL STEPS COMPLETED!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Production package ready:" -ForegroundColor Cyan
    Write-Host "  $ProjectRoot\deployment\DongleSyncSystem-v1.0.0.zip" -ForegroundColor White
    Write-Host ""
    Write-Host "Package contains:" -ForegroundColor Yellow
    Write-Host "  [OK] MSI Installer (for end users)" -ForegroundColor Gray
    Write-Host "  [OK] Standalone DongleCreatorTool (for admins)" -ForegroundColor Gray
    Write-Host "  [OK] Complete documentation" -ForegroundColor Gray
    Write-Host "  [OK] README with installation guide" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Ready to distribute!" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "BUILD FAILED" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please check error messages above" -ForegroundColor Yellow
    exit 1
}

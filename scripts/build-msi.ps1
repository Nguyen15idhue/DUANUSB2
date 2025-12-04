# PowerShell Script: Build WiX MSI Installer
# Purpose: Compile Product.wxs into MSI installer package

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Building WiX MSI Installer" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$ProjectRoot = "F:\3.Laptrinh\DUANUSB2"
$InstallerDir = "$ProjectRoot\installer"
$ObjDir = "$InstallerDir\obj"
$BinDir = "$InstallerDir\bin"
$ServiceBinDir = "$ProjectRoot\src\DongleSyncService\bin\Release\net8.0"

# Detect WiX Toolset version
$WixPath = $null
$WixVersion = $null

if (Test-Path "C:\Program Files (x86)\WiX Toolset v3.11\bin\candle.exe") {
    $WixPath = "C:\Program Files (x86)\WiX Toolset v3.11\bin"
    $WixVersion = 3
    Write-Host "Detected: WiX Toolset v3.11" -ForegroundColor Green
    Write-Host ""
} elseif (Test-Path "C:\Program Files\WiX Toolset v6.0\bin\wix.exe") {
    Write-Host ""
    Write-Host "INCOMPATIBILITY WARNING:" -ForegroundColor Yellow
    Write-Host "  Found WiX Toolset v6.0, but this project requires WiX v3.11" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Why v3.11?" -ForegroundColor Cyan
    Write-Host "  - Product.wxs is written for WiX v3.x syntax" -ForegroundColor Gray
    Write-Host "  - WiX v6.0 has breaking changes (new namespace, different structure)" -ForegroundColor Gray
    Write-Host "  - Converting requires significant rewrite of installer definition" -ForegroundColor Gray
    Write-Host ""
    Write-Host "SOLUTION:" -ForegroundColor Green
    Write-Host "  1. Download WiX v3.11 from:" -ForegroundColor White
    Write-Host "     https://github.com/wixtoolset/wix3/releases/tag/wix3112rtm" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  2. Install wix311.exe (both v3.11 and v6.0 can coexist)" -ForegroundColor White
    Write-Host ""
    Write-Host "  3. Re-run this script" -ForegroundColor White
    Write-Host ""
    exit 1
} else {
    Write-Host ""
    Write-Host "ERROR: WiX Toolset v3.11 not found" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install WiX Toolset v3.11 from:" -ForegroundColor Yellow
    Write-Host "  https://github.com/wixtoolset/wix3/releases/tag/wix3112rtm" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Download and run: wix311.exe" -ForegroundColor White
    Write-Host ""
    exit 1
}

# Check if service is built
if (-not (Test-Path "$ServiceBinDir\DongleSyncService.exe")) {
    Write-Host "ERROR: DongleSyncService.exe not found" -ForegroundColor Red
    Write-Host "Please build the service first:" -ForegroundColor Yellow
    Write-Host "  cd $ProjectRoot\src\DongleSyncService" -ForegroundColor White
    Write-Host "  dotnet build -c Release" -ForegroundColor White
    exit 1
}

# Create output directories
New-Item -ItemType Directory -Path $ObjDir -Force | Out-Null
New-Item -ItemType Directory -Path $BinDir -Force | Out-Null

# Step 1: Compile WXS to WIXOBJ
Write-Host "Step 1: Compiling Product.wxs..." -ForegroundColor Green
Write-Host "  Input:  $InstallerDir\Product.wxs" -ForegroundColor Gray
Write-Host "  Output: $ObjDir\Product.wixobj" -ForegroundColor Gray
Write-Host ""

try {
    & "$WixPath\candle.exe" `
        "$InstallerDir\Product.wxs" `
        -out "$ObjDir\Product.wixobj" `
        -arch x64 `
        -ext WixUtilExtension `
        -dSolutionDir="$ProjectRoot\"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "ERROR: Candle compilation failed" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host ""
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "[OK] Compilation successful" -ForegroundColor Green
Write-Host ""

# Step 2: Link WIXOBJ to MSI
Write-Host "Step 2: Linking to MSI..." -ForegroundColor Green
Write-Host "  Input:  $ObjDir\Product.wixobj" -ForegroundColor Gray
Write-Host "  Output: $BinDir\DongleSyncService-Setup.msi" -ForegroundColor Gray
Write-Host ""

try {
    & "$WixPath\light.exe" `
        "$ObjDir\Product.wixobj" `
        -out "$BinDir\DongleSyncService-Setup.msi" `
        -ext WixUIExtension `
        -ext WixUtilExtension `
        -cultures:en-US `
        -sice:ICE61
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "ERROR: Light linking failed" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host ""
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "[OK] Linking successful" -ForegroundColor Green
Write-Host ""

# Success
Write-Host "========================================" -ForegroundColor Green
Write-Host "[OK] MSI Installer Built Successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Installer location:" -ForegroundColor Cyan
Write-Host "  $BinDir\DongleSyncService-Setup.msi" -ForegroundColor White
Write-Host ""

# Show file size
$msiFile = Get-Item "$BinDir\DongleSyncService-Setup.msi"
$sizeMB = [math]::Round($msiFile.Length / 1MB, 2)
Write-Host "File size: $sizeMB MB" -ForegroundColor Gray
Write-Host ""
Write-Host "The MSI installer includes:" -ForegroundColor Yellow
Write-Host "  - Windows Service installation" -ForegroundColor Gray
Write-Host "  - Automatic startup configuration" -ForegroundColor Gray
Write-Host "  - Start menu shortcuts" -ForegroundColor Gray
Write-Host "  - Service recovery settings" -ForegroundColor Gray
Write-Host ""

# PowerShell Script: Harvest files and build MSI installer
# Purpose: Generate ServiceComponents.wxs from publish folder and build MSI

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Harvesting Files and Building MSI" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$ProjectRoot = "F:\3.Laptrinh\DUANUSB2"
$PublishDir = "$ProjectRoot\src\DongleSyncService\bin\Release\net8.0\win-x64\publish"
$InstallerDir = "$ProjectRoot\installer"
$ObjDir = "$InstallerDir\obj"
$BinDir = "$InstallerDir\bin"

# WiX paths
$WixPath = "C:\Program Files (x86)\WiX Toolset v3.11\bin"

# Check WiX installation
if (-not (Test-Path "$WixPath\heat.exe")) {
    Write-Host "ERROR: WiX Toolset v3.11 not found at: $WixPath" -ForegroundColor Red
    Write-Host "Please install from: https://github.com/wixtoolset/wix3/releases/tag/wix3112rtm" -ForegroundColor Yellow
    exit 1
}

# Check publish folder
if (-not (Test-Path "$PublishDir\DongleSyncService.exe")) {
    Write-Host "ERROR: Published files not found at: $PublishDir" -ForegroundColor Red
    Write-Host "Please run dotnet publish first" -ForegroundColor Yellow
    exit 1
}

# Create output directories
New-Item -ItemType Directory -Path $ObjDir -Force | Out-Null
New-Item -ItemType Directory -Path $BinDir -Force | Out-Null

# Count files to harvest
$fileCount = (Get-ChildItem "$PublishDir" -File).Count
Write-Host "Found $fileCount files to harvest from publish folder" -ForegroundColor Green
Write-Host ""

# Step 1: Harvest files with Heat
Write-Host "Step 1: Harvesting files with Heat.exe..." -ForegroundColor Green
Write-Host "  Input:  $PublishDir" -ForegroundColor Gray
Write-Host "  Output: $InstallerDir\ServiceComponents.wxs" -ForegroundColor Gray
Write-Host ""

try {
    & "$WixPath\heat.exe" dir "$PublishDir" `
        -cg ServiceComponents `
        -gg `
        -scom `
        -sreg `
        -sfrag `
        -srd `
        -dr INSTALLFOLDER `
        -var var.PublishDir `
        -out "$InstallerDir\ServiceComponents.wxs"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "ERROR: Heat harvesting failed" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host ""
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "[OK] Harvested successfully" -ForegroundColor Green
Write-Host ""

# Step 2: Compile Product.wxs
Write-Host "Step 2: Compiling Product.wxs..." -ForegroundColor Green

try {
    & "$WixPath\candle.exe" `
        "$InstallerDir\Product.wxs" `
        -out "$ObjDir\Product.wixobj" `
        -arch x64 `
        -ext WixUtilExtension `
        -dSolutionDir="$ProjectRoot\" `
        -dPublishDir="$PublishDir"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "ERROR: Product.wxs compilation failed" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host ""
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "[OK] Product.wxs compiled" -ForegroundColor Green
Write-Host ""

# Step 3: Compile ServiceComponents.wxs
Write-Host "Step 3: Compiling ServiceComponents.wxs..." -ForegroundColor Green

try {
    & "$WixPath\candle.exe" `
        "$InstallerDir\ServiceComponents.wxs" `
        -out "$ObjDir\ServiceComponents.wixobj" `
        -arch x64 `
        -ext WixUtilExtension `
        -dPublishDir="$PublishDir"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "ERROR: ServiceComponents.wxs compilation failed" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host ""
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "[OK] ServiceComponents.wxs compiled" -ForegroundColor Green
Write-Host ""

# Step 4: Link to MSI
Write-Host "Step 4: Linking to MSI..." -ForegroundColor Green
Write-Host "  Linking: Product.wixobj + ServiceComponents.wixobj" -ForegroundColor Gray
Write-Host "  Output:  $BinDir\DongleSyncService-Setup.msi" -ForegroundColor Gray
Write-Host ""

try {
    & "$WixPath\light.exe" `
        "$ObjDir\Product.wixobj" `
        "$ObjDir\ServiceComponents.wixobj" `
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
Write-Host "Includes: $fileCount service files + dependencies" -ForegroundColor Gray
Write-Host ""

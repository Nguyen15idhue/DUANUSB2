# Build MSI Installer cho DongleSyncService
# Sử dụng WiX Toolset v5

$ErrorActionPreference = "Stop"

# Định nghĩa paths
$projectRoot = "F:\3.Laptrinh\DUANUSB2"
$installerDir = "$projectRoot\installer"
$buildDir = "$installerDir\build"
$outputDir = "$installerDir\output"

# Tạo output directory
if (!(Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}

Write-Host "Building MSI installer..." -ForegroundColor Green

# Build WiX
try {
    Push-Location $installerDir
    
    # Compile WiX source
    Write-Host "Compiling WiX source..." -ForegroundColor Yellow
    wix build -arch x64 `
        -ext WixToolset.UI.wixext `
        -d SourceDir="$buildDir" `
        -out "$outputDir\DongleSyncService.msi" `
        DongleService.wxs
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n✅ MSI created successfully!" -ForegroundColor Green
        Write-Host "📦 Location: $outputDir\DongleSyncService.msi" -ForegroundColor Cyan
        
        # Show file info
        $msiFile = Get-Item "$outputDir\DongleSyncService.msi"
        Write-Host "`nFile size: $([math]::Round($msiFile.Length / 1MB, 2)) MB" -ForegroundColor Gray
    } else {
        Write-Host "❌ Build failed!" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "❌ Error: $_" -ForegroundColor Red
    exit 1
}
finally {
    Pop-Location
}

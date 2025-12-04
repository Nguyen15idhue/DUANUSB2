# PowerShell Script: Convert PNG to ICO and Setup Application Icons
# Purpose: Convert logo images to ICO format for MSI and application icons

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Converting Logo Images to ICO Format" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$ProjectRoot = "F:\3.Laptrinh\DUANUSB2"
$ServiceLogoPath = "$ProjectRoot\Treetog-I-Image-File.256.png"
$DevLogoPath = "$ProjectRoot\Treetog-I-Image-File.256.png"

$ServiceIcoOutput = "$ProjectRoot\src\DongleSyncService\icon.ico"
$DevIcoOutput = "$ProjectRoot\src\DongleCreatorTool\icon.ico"

# Check if source images exist
if (-not (Test-Path $ServiceLogoPath)) {
    Write-Host "ERROR: Service logo not found at: $ServiceLogoPath" -ForegroundColor Red
    exit 1
}

Write-Host "[OK] Source images found" -ForegroundColor Green
Write-Host ""

# Function to convert PNG to ICO using .NET
function Convert-PngToIco {
    param(
        [string]$PngPath,
        [string]$IcoPath,
        [string]$Description
    )
    
    Write-Host "Converting $Description..." -ForegroundColor Yellow
    Write-Host "  Input:  $PngPath" -ForegroundColor Gray
    Write-Host "  Output: $IcoPath" -ForegroundColor Gray
    
    try {
        Add-Type -AssemblyName System.Drawing
        
        $png = [System.Drawing.Image]::FromFile($PngPath)
        $bitmap = New-Object System.Drawing.Bitmap(256, 256)
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.DrawImage($png, 0, 0, 256, 256)
        
        $iconHandle = $bitmap.GetHicon()
        $icon = [System.Drawing.Icon]::FromHandle($iconHandle)
        
        $iconStream = [System.IO.FileStream]::new($IcoPath, [System.IO.FileMode]::Create)
        $icon.Save($iconStream)
        $iconStream.Close()
        
        $graphics.Dispose()
        $bitmap.Dispose()
        $png.Dispose()
        $icon.Dispose()
        
        Write-Host "  [OK] Conversion successful" -ForegroundColor Green
        Write-Host ""
        return $true
    }
    catch {
        Write-Host "  ERROR: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host ""
        return $false
    }
}

# Convert both logos
$serviceSuccess = Convert-PngToIco -PngPath $ServiceLogoPath -IcoPath $ServiceIcoOutput -Description "Service/MSI Logo"
$devSuccess = Convert-PngToIco -PngPath $DevLogoPath -IcoPath $DevIcoOutput -Description "Dev Tool Logo"

if (-not $serviceSuccess -or -not $devSuccess) {
    Write-Host "ERROR: Conversion failed" -ForegroundColor Red
    Write-Host "Use online converter: https://www.icoconverter.com/" -ForegroundColor Yellow
    exit 1
}

Write-Host "========================================" -ForegroundColor Green
Write-Host "[OK] Icon Conversion Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Icon files created at:" -ForegroundColor Cyan
Write-Host "  [OK] $ServiceIcoOutput" -ForegroundColor White
Write-Host "  [OK] $DevIcoOutput" -ForegroundColor White
Write-Host ""
Write-Host "Next: Updating project files..." -ForegroundColor Yellow

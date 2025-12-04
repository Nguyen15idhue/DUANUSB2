# PowerShell Script: Publish DongleCreatorTool as standalone EXE
# Purpose: Creates self-contained single-file executable for distribution

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Publishing DongleCreatorTool Standalone" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$ProjectPath = "F:\3.Laptrinh\DUANUSB2\src\DongleCreatorTool"
$OutputDir = "F:\3.Laptrinh\DUANUSB2\output\DongleCreatorTool"
$Runtime = "win-x64"

# Check if project exists
if (-not (Test-Path "$ProjectPath\DongleCreatorTool.csproj")) {
    Write-Host "ERROR: Project file not found at $ProjectPath" -ForegroundColor Red
    exit 1
}

# Clean previous output
if (Test-Path $OutputDir) {
    Write-Host "Cleaning previous output..." -ForegroundColor Yellow
    Remove-Item -Path $OutputDir -Recurse -Force
}

# Create output directory
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

# Publish as self-contained single file
Write-Host "Publishing project..." -ForegroundColor Green
Write-Host "Runtime: $Runtime" -ForegroundColor Gray
Write-Host "Output: $OutputDir" -ForegroundColor Gray
Write-Host ""

try {
    & dotnet publish "$ProjectPath\DongleCreatorTool.csproj" `
        -c Release `
        -r $Runtime `
        --self-contained true `
        /p:PublishSingleFile=true `
        /p:IncludeNativeLibrariesForSelfExtract=true `
        /p:EnableCompressionInSingleFile=true `
        /p:IncludeAllContentForSelfExtract=true `
        -o $OutputDir
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "[OK] Publish Successful!" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "Output location:" -ForegroundColor Cyan
        Write-Host "  $OutputDir\DongleCreatorTool.exe" -ForegroundColor White
        Write-Host ""
        
        # Show file size
        $exeFile = Get-Item "$OutputDir\DongleCreatorTool.exe"
        $sizeMB = [math]::Round($exeFile.Length / 1MB, 2)
        Write-Host "File size: $sizeMB MB" -ForegroundColor Gray
        Write-Host ""
        Write-Host "This is a standalone executable that includes:" -ForegroundColor Yellow
        Write-Host "  - .NET 8.0 Runtime" -ForegroundColor Gray
        Write-Host "  - All required dependencies" -ForegroundColor Gray
        Write-Host "  - No installation needed" -ForegroundColor Gray
        Write-Host ""
    } else {
        Write-Host ""
        Write-Host "ERROR: Publish failed with exit code $($process.ExitCode)" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host ""
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

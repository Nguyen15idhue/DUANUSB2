# ============================================
# BUILD INNO SETUP INSTALLER
# Creates professional GUI installer (.exe)
# ============================================

param(
    [switch]$SkipBuild,
    [switch]$OpenOutput
)

$ErrorActionPreference = "Stop"

# Colors
$ErrorColor = "Red"
$SuccessColor = "Green"
$InfoColor = "Cyan"
$WarningColor = "Yellow"

Write-Host "========================================" -ForegroundColor $InfoColor
Write-Host "  INNO SETUP INSTALLER BUILDER" -ForegroundColor $InfoColor
Write-Host "========================================" -ForegroundColor $InfoColor
Write-Host ""

# ============================================
# STEP 1: Check Inno Setup Installation
# ============================================

Write-Host "[*] Checking for Inno Setup..." -ForegroundColor $InfoColor

$InnoSetupPaths = @(
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    "C:\Program Files\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles}\Inno Setup 6\ISCC.exe"
)

$InnoSetupCompiler = $null
foreach ($path in $InnoSetupPaths) {
    if (Test-Path $path) {
        $InnoSetupCompiler = $path
        Write-Host "[+] Found Inno Setup: $path" -ForegroundColor $SuccessColor
        break
    }
}

if (-not $InnoSetupCompiler) {
    Write-Host "[!] ERROR: Inno Setup not found!" -ForegroundColor $ErrorColor
    Write-Host ""
    Write-Host "Please download and install Inno Setup 6:" -ForegroundColor $WarningColor
    Write-Host "https://jrsoftware.org/isdl.php" -ForegroundColor White
    Write-Host ""
    Write-Host "Download: innosetup-6.x.x.exe (Unicode version)" -ForegroundColor White
    Write-Host ""
    Read-Host "Press Enter to exit"
    exit 1
}

# ============================================
# STEP 2: Build Service (if not skipped)
# ============================================

if (-not $SkipBuild) {
    Write-Host ""
    Write-Host "[*] Building DongleSyncService..." -ForegroundColor $InfoColor
    
    Push-Location "F:\3.Laptrinh\DUANUSB2\src\DongleSyncService"
    
    $buildResult = dotnet publish -c Release -r win-x64 --self-contained true --nologo 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[!] Build failed!" -ForegroundColor $ErrorColor
        Write-Host $buildResult -ForegroundColor $ErrorColor
        Pop-Location
        Read-Host "Press Enter to exit"
        exit 1
    }
    
    Pop-Location
    Write-Host "[+] Build completed successfully" -ForegroundColor $SuccessColor
} else {
    Write-Host "[*] Skipping build (using existing binaries)" -ForegroundColor $WarningColor
}

# ============================================
# STEP 3: Verify Required Files
# ============================================

Write-Host ""
Write-Host "[*] Verifying required files..." -ForegroundColor $InfoColor

$publishDir = "F:\3.Laptrinh\DUANUSB2\src\DongleSyncService\bin\Release\net8.0\win-x64\publish"
$issFile = "F:\3.Laptrinh\DUANUSB2\installer\DongleSyncService-Setup.iss"
$iconFile = "F:\3.Laptrinh\DUANUSB2\installer\icon.ico"
$licenseFile = "F:\3.Laptrinh\DUANUSB2\installer\License.rtf"

$requiredFiles = @{
    "Service Executable" = "$publishDir\DongleSyncService.exe"
    "DLLPatch.dll" = "$publishDir\DLLPatch.dll"
    "Inno Setup Script" = $issFile
    "Icon File" = $iconFile
    "License File" = $licenseFile
}

$missingFiles = @()
foreach ($item in $requiredFiles.GetEnumerator()) {
    if (Test-Path $item.Value) {
        Write-Host "   [+] $($item.Key)" -ForegroundColor Gray
    } else {
        Write-Host "   [!] $($item.Key): NOT FOUND" -ForegroundColor $ErrorColor
        Write-Host "      Expected: $($item.Value)" -ForegroundColor Gray
        $missingFiles += $item.Key
    }
}

if ($missingFiles.Count -gt 0) {
    Write-Host ""
    Write-Host "[!] Missing required files!" -ForegroundColor $ErrorColor
    Write-Host "Please ensure all files exist before building installer." -ForegroundColor $WarningColor
    Read-Host "Press Enter to exit"
    exit 1
}

# ============================================
# STEP 4: Create Output Directory
# ============================================

Write-Host ""
Write-Host "[*] Preparing output directory..." -ForegroundColor $InfoColor

$outputDir = "F:\3.Laptrinh\DUANUSB2\output"
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
    Write-Host "   Created: $outputDir" -ForegroundColor Gray
}

# ============================================
# STEP 5: Compile Inno Setup Installer
# ============================================

Write-Host ""
Write-Host "[*] Compiling Inno Setup installer..." -ForegroundColor $InfoColor
Write-Host "   This may take a minute..." -ForegroundColor Gray
Write-Host ""

$compileResult = & $InnoSetupCompiler $issFile 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "[!] Installer compilation failed!" -ForegroundColor $ErrorColor
    Write-Host $compileResult -ForegroundColor $ErrorColor
    Read-Host "Press Enter to exit"
    exit 1
}

# ============================================
# STEP 6: Verify Output
# ============================================

$installerPattern = "$outputDir\DongleSyncService-Setup-v*.exe"
$installerFile = Get-ChildItem -Path $installerPattern -ErrorAction SilentlyContinue | Select-Object -First 1

if ($installerFile) {
    $fileSize = [math]::Round($installerFile.Length / 1MB, 2)
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor $SuccessColor
    Write-Host "  [+] INSTALLER CREATED SUCCESSFULLY!" -ForegroundColor $SuccessColor
    Write-Host "========================================" -ForegroundColor $SuccessColor
    Write-Host ""
    Write-Host "Installer Details:" -ForegroundColor White
    Write-Host "   File:     $($installerFile.Name)" -ForegroundColor Gray
    Write-Host "   Size:     $fileSize MB" -ForegroundColor Gray
    Write-Host "   Location: $($installerFile.FullName)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Ready for distribution!" -ForegroundColor $SuccessColor
    Write-Host ""
    Write-Host "To test installer:" -ForegroundColor $WarningColor
    Write-Host "   1. Right-click installer file" -ForegroundColor White
    Write-Host "   2. Select 'Run as administrator'" -ForegroundColor White
    Write-Host "   3. Follow installation wizard" -ForegroundColor White
    Write-Host ""
    
    # Open output folder if requested
    if ($OpenOutput) {
        Start-Process "explorer.exe" -ArgumentList "/select,`"$($installerFile.FullName)`""
    }
} else {
    Write-Host "[!] Installer file not found in output directory!" -ForegroundColor $WarningColor
    Write-Host "Expected pattern: $installerPattern" -ForegroundColor Gray
}

Write-Host "Press Enter to exit..."
Read-Host

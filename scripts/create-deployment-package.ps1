# PowerShell Script: Create Deployment Package
# Purpose: Bundle MSI installer and standalone tool for distribution

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Creating Deployment Package" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$ProjectRoot = "F:\3.Laptrinh\DUANUSB2"
$MsiPath = "$ProjectRoot\installer\bin\DongleSyncService-Setup.msi"
$ExePath = "$ProjectRoot\output\DongleCreatorTool\DongleCreatorTool.exe"
$DeploymentDir = "$ProjectRoot\deployment"
$Version = "1.0.0"
$PackageName = "DongleSyncSystem-v$Version"
$PackageDir = "$DeploymentDir\$PackageName"

# Validate inputs
Write-Host "Validating package components..." -ForegroundColor Yellow

if (-not (Test-Path $MsiPath)) {
    Write-Host "ERROR: MSI installer not found" -ForegroundColor Red
    Write-Host "Please run: .\scripts\build-msi.ps1" -ForegroundColor White
    exit 1
}

if (-not (Test-Path $ExePath)) {
    Write-Host "ERROR: DongleCreatorTool.exe not found" -ForegroundColor Red
    Write-Host "Please run: .\scripts\publish-dongle-creator.ps1" -ForegroundColor White
    exit 1
}

Write-Host "[OK] All components found" -ForegroundColor Green
Write-Host ""

# Clean previous deployment
if (Test-Path $PackageDir) {
    Write-Host "Cleaning previous deployment..." -ForegroundColor Yellow
    Remove-Item -Path $PackageDir -Recurse -Force
}

# Create deployment directory structure
Write-Host "Creating deployment structure..." -ForegroundColor Green
New-Item -ItemType Directory -Path "$PackageDir\Installer" -Force | Out-Null
New-Item -ItemType Directory -Path "$PackageDir\Tools" -Force | Out-Null
New-Item -ItemType Directory -Path "$PackageDir\Documentation" -Force | Out-Null

# Copy MSI installer
Write-Host "Copying MSI installer..." -ForegroundColor Green
Copy-Item -Path $MsiPath -Destination "$PackageDir\Installer\DongleSyncService-Setup.msi"

# Copy standalone tool
Write-Host "Copying DongleCreatorTool..." -ForegroundColor Green
Copy-Item -Path $ExePath -Destination "$PackageDir\Tools\DongleCreatorTool.exe"

# Copy documentation
Write-Host "Copying documentation..." -ForegroundColor Green
$DocsPath = "$ProjectRoot\docs"
if (Test-Path "$DocsPath\SERVICE-INSTALLATION.md") {
    Copy-Item -Path "$DocsPath\SERVICE-INSTALLATION.md" -Destination "$PackageDir\Documentation\"
}
if (Test-Path "$DocsPath\FINAL-SECURE-SOLUTION.md") {
    Copy-Item -Path "$DocsPath\FINAL-SECURE-SOLUTION.md" -Destination "$PackageDir\Documentation\"
}

# Create README
Write-Host "Creating deployment README..." -ForegroundColor Green
$ReadmeContent = @"
# USB Dongle Sync System - Deployment Package v$Version
**Date: $(Get-Date -Format 'yyyy-MM-dd')**

## Package Contents

This deployment package contains two main components:

### 1. DongleSyncService-Setup.msi (Installer)
- **Location**: `Installer\DongleSyncService-Setup.msi`
- **Purpose**: Windows Service for USB dongle validation and DLL synchronization
- **Target Users**: End users who need to use USB dongles for application protection
- **Installation**: Double-click MSI file and follow installation wizard
- **System Requirements**: Windows 10/11, Administrator privileges, .NET 8.0 Runtime

### 2. DongleCreatorTool.exe (Standalone Tool)
- **Location**: `Tools\DongleCreatorTool.exe`
- **Purpose**: Create and configure USB dongles with encrypted DLL payload
- **Target Users**: Administrators, developers, IT staff
- **Usage**: Run directly, no installation required
- **System Requirements**: Windows 10/11, Administrator privileges

---

## Quick Start Guide

### For End Users (Installing the Service)

1. **Run the MSI installer**
   - Navigate to: `Installer\DongleSyncService-Setup.msi`
   - Double-click to launch installer
   - Follow on-screen instructions
   - Service will start automatically after installation

2. **Verify installation**
   - Service should appear in Windows Services as "USB Dongle Sync Service"
   - Status: Running
   - Startup Type: Automatic

3. **Insert USB dongle**
   - Insert your authorized USB dongle
   - Service will automatically detect and validate
   - If valid, encrypted DLL will be synced to: `C:\Program Files\YourApp\patch.dll`

### For Administrators (Creating Dongles)

1. **Run DongleCreatorTool**
   - Navigate to: `Tools\DongleCreatorTool.exe`
   - Right-click > Run as Administrator
   - Tool will open without installation

2. **Create dongle**
   - Insert blank USB drive
   - Select USB drive letter
   - Browse and select DLL file to encrypt
   - Click "Create Dongle"
   - Tool will generate unique hardware-bound dongle

3. **Bind to machine (optional)**
   - To bind dongle to specific machine
   - Insert dongle on target computer
   - Service will create bind.key automatically on first use
   - Dongle will only work on that machine afterward

---

## Installation Steps (Detailed)

### Service Installation

1. **Prerequisites**
   - Windows 10 or Windows 11
   - Administrator privileges
   - .NET 8.0 Runtime (included in installer)

2. **Installation Process**
   ```
   1. Double-click DongleSyncService-Setup.msi
   2. Click "Next" on welcome screen
   3. Accept license agreement
   4. Choose installation folder (default: C:\Program Files\DongleSyncService)
   5. Click "Install"
   6. Wait for installation to complete
   7. Click "Finish"
   ```

3. **Post-Installation Verification**
   - Press Win+R, type: `services.msc`
   - Find "USB Dongle Sync Service"
   - Status should be "Running"
   - Startup Type should be "Automatic"

4. **Check Logs**
   - Log location: `C:\ProgramData\DongleSyncService\logs\`
   - View latest log: `log-YYYYMMDD.txt`
   - Look for: `Service started successfully`

### Service Uninstallation

1. **Using Control Panel**
   ```
   1. Open Control Panel
   2. Programs > Programs and Features
   3. Find "USB Dongle Sync Service"
   4. Right-click > Uninstall
   5. Follow uninstall wizard
   ```

2. **Data Cleanup (Optional)**
   - Service data: `C:\ProgramData\DongleSyncService\`
   - Contains: logs, bind.key, dongle.key
   - Delete manually if needed

---

## DongleCreatorTool Usage

### Creating a New Dongle

1. **Prepare USB Drive**
   - Insert clean USB drive (FAT32 or NTFS)
   - Note the drive letter (e.g., D:)
   - Backup any data (will not be deleted, but dongle files added)

2. **Launch Tool**
   - Navigate to: `Tools\DongleCreatorTool.exe`
   - Right-click > "Run as Administrator"
   - Tool opens immediately (no installation)

3. **Configure Dongle**
   - **USB Drive**: Select drive letter from dropdown
   - **DLL File**: Browse and select the DLL to protect
   - Click "Create Dongle" button

4. **Dongle Creation Process**
   ```
   - Generates unique GUID for dongle
   - Reads USB hardware ID (PNPDeviceID)
   - Encrypts DLL with AES-256
   - Creates dongle files on USB:
     * config.json (dongle metadata)
     * dongle.key (hardware key)
     * patch.dll.enc (encrypted DLL)
     * iv.bin (encryption IV)
   ```

5. **Success**
   - Tool displays "Dongle created successfully!"
   - USB drive now authorized
   - Any previous bind.key automatically deleted

### Important Notes

- **Hardware Binding**: Dongle is bound to USB's physical hardware ID
- **One Dongle Per USB**: Each USB creates unique dongle
- **Machine Binding**: First use creates bind.key on that machine
- **Security**: AES-256 encryption, cannot be copied
- **Persistence**: Dongle files cannot be duplicated to other USBs

---

## Troubleshooting

### Service Issues

**Service won't start**
- Check Event Viewer: Windows Logs > Application
- Verify .NET 8.0 Runtime installed
- Run as Administrator: `sc start DongleSyncService`

**Dongle not detected**
- Check service logs: `C:\ProgramData\DongleSyncService\logs\`
- Verify USB drive letter
- Ensure dongle files exist on USB

**DLL not syncing**
- Check target path: `C:\Program Files\YourApp\patch.dll`
- Verify target directory exists
- Check permissions (service runs as LocalSystem)

### DongleCreatorTool Issues

**"Access Denied" error**
- Run tool as Administrator
- Check USB drive is not write-protected

**"USB not found" error**
- Ensure USB drive inserted
- Check drive appears in File Explorer
- Try different USB port

**"Cannot read DLL" error**
- Verify DLL file exists
- Check file is not locked by another process
- Ensure DLL is valid PE file

---

## Technical Specifications

### Service Details
- **Name**: DongleSyncService
- **Display Name**: USB Dongle Sync Service
- **Account**: LocalSystem
- **Startup**: Automatic
- **Recovery**: Restart on failure (3 attempts)

### Security Features
- AES-256-CBC encryption for DLL
- SHA256 hardware key generation
- PNPDeviceID-based USB identification
- Machine binding via bind.key
- Encrypted storage on USB dongle

### System Requirements
- Windows 10 version 1809 or later
- Windows 11 (all versions)
- .NET 8.0 Runtime
- Administrator privileges
- USB 2.0/3.0 port

---

## Support & Documentation

### Documentation Files
- `SERVICE-INSTALLATION.md` - Complete service management guide
- `FINAL-SECURE-SOLUTION.md` - Security architecture details

### Log Locations
- Service logs: `C:\ProgramData\DongleSyncService\logs\`
- Windows Event Viewer: Application logs

### Contact Information
- For technical support, contact your system administrator
- For development issues, refer to project documentation

---

## Version History

### v1.0.0 (2025-12-04)
- Initial production release
- Windows Service installer (MSI)
- Standalone DongleCreatorTool
- PNPDeviceID-based hardware validation
- Auto-delete bind.key feature
- Complete documentation

---

## License & Copyright

**Copyright © 2025. All rights reserved.**

This software is proprietary and confidential. Unauthorized copying, distribution, or use is strictly prohibited.

---

**End of README**
"@

Set-Content -Path "$PackageDir\README.md" -Value $ReadmeContent -Encoding UTF8

# Create ZIP archive
Write-Host "Creating ZIP archive..." -ForegroundColor Green
$ZipPath = "$DeploymentDir\$PackageName.zip"
if (Test-Path $ZipPath) {
    Remove-Item -Path $ZipPath -Force
}
Compress-Archive -Path "$PackageDir\*" -DestinationPath $ZipPath

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "[OK] Deployment Package Created!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Package location:" -ForegroundColor Cyan
Write-Host "  $ZipPath" -ForegroundColor White
Write-Host ""
Write-Host "Package structure:" -ForegroundColor Cyan
Write-Host "  Installer/" -ForegroundColor White
Write-Host "    DongleSyncService-Setup.msi" -ForegroundColor Gray
Write-Host "  Tools/" -ForegroundColor White
Write-Host "    DongleCreatorTool.exe" -ForegroundColor Gray
Write-Host "  Documentation/" -ForegroundColor White
Write-Host "    SERVICE-INSTALLATION.md" -ForegroundColor Gray
Write-Host "    FINAL-SECURE-SOLUTION.md" -ForegroundColor Gray
Write-Host "  README.md" -ForegroundColor White
Write-Host ""

# Show archive size
$zipFile = Get-Item $ZipPath
$sizeMB = [math]::Round($zipFile.Length / 1MB, 2)
Write-Host "Archive size: $sizeMB MB" -ForegroundColor Gray
Write-Host ""
Write-Host "Ready for distribution" -ForegroundColor Yellow
Write-Host ""

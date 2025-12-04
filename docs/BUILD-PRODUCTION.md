# Building Production Package

## Overview

This guide explains how to build the complete production package for the USB Dongle Sync System.

## Prerequisites

1. **.NET 8.0 SDK** - For building projects
2. **WiX Toolset 3.11** - For creating MSI installer
   - Download from: https://wixtoolset.org/releases/
   - Install to default location: `C:\Program Files (x86)\WiX Toolset v3.11\`
3. **PowerShell 5.1+** - For running build scripts

## Quick Build (Recommended)

Run the master build script to execute all steps automatically:

```powershell
cd F:\3.Laptrinh\DUANUSB2
.\scripts\build-all.ps1
```

This will:
1. Build DongleSyncService (Release)
2. Publish DongleCreatorTool as standalone EXE
3. Build MSI installer
4. Create deployment package ZIP

**Output**: `deployment\DongleSyncSystem-v1.0.0.zip`

## Step-by-Step Build

If you prefer to run each step individually:

### 1. Build DongleSyncService

```powershell
cd F:\3.Laptrinh\DUANUSB2\src\DongleSyncService
dotnet build -c Release
```

**Output**: `bin\Release\net8.0\DongleSyncService.exe`

### 2. Publish DongleCreatorTool

```powershell
cd F:\3.Laptrinh\DUANUSB2
.\scripts\publish-dongle-creator.ps1
```

**Output**: `output\DongleCreatorTool\DongleCreatorTool.exe` (standalone, ~60MB)

### 3. Build MSI Installer

```powershell
cd F:\3.Laptrinh\DUANUSB2
.\scripts\build-msi.ps1
```

**Output**: `installer\bin\DongleSyncService-Setup.msi`

### 4. Create Deployment Package

```powershell
cd F:\3.Laptrinh\DUANUSB2
.\scripts\create-deployment-package.ps1
```

**Output**: `deployment\DongleSyncSystem-v1.0.0.zip`

## Deployment Package Contents

The final ZIP package includes:

```
DongleSyncSystem-v1.0.0/
├── Installer/
│   └── DongleSyncService-Setup.msi    # Windows Service installer
├── Tools/
│   └── DongleCreatorTool.exe          # Standalone dongle creator
├── Documentation/
│   ├── SERVICE-INSTALLATION.md        # End-user guide
│   └── FINAL-SECURE-SOLUTION.md       # Technical details
└── README.md                          # Package overview
```

## Troubleshooting

### WiX Toolset Not Found

If `build-msi.ps1` fails with "WiX Toolset not found":

1. Install WiX Toolset 3.11 from https://wixtoolset.org/releases/
2. Or update `$WixPath` in script if installed elsewhere

### Icon File Missing

The installer references `src\DongleSyncService\icon.ico`. Currently a placeholder exists. To add a real icon:

1. Create 256x256 PNG image
2. Convert to ICO format using https://www.icoconverter.com/
3. Replace `src\DongleSyncService\icon.ico`
4. Rebuild MSI

### Build Warnings

Build may show 91 warnings about:
- CS8618: Nullable reference types
- CA1416: Platform-specific APIs

These are **non-blocking** and can be ignored.

### Process Lock Errors

If build fails with "file is being used by another process":

```powershell
# Stop running service
sc stop DongleSyncService

# Kill processes
Get-Process DongleSyncService -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process DongleCreatorTool -ErrorAction SilentlyContinue | Stop-Process -Force

# Rebuild
.\scripts\build-all.ps1
```

## Version Management

To change version number:

1. Edit `scripts\create-deployment-package.ps1`
2. Update `$Version = "1.0.0"` to desired version
3. Rebuild package

## Distribution

Once built, distribute `DongleSyncSystem-v1.0.0.zip` to end users. They can extract and follow README.md instructions for installation.

### For End Users
- Extract ZIP
- Run `Installer\DongleSyncService-Setup.msi`
- Follow installation wizard

### For Administrators/Developers
- Extract ZIP
- Run `Tools\DongleCreatorTool.exe` (no installation needed)
- Create dongles as needed

## Clean Build

To clean all build outputs before rebuilding:

```powershell
# Clean DongleSyncService
cd F:\3.Laptrinh\DUANUSB2\src\DongleSyncService
dotnet clean
Remove-Item -Path "bin","obj" -Recurse -Force -ErrorAction SilentlyContinue

# Clean DongleCreatorTool
cd F:\3.Laptrinh\DUANUSB2\src\DongleCreatorTool
dotnet clean
Remove-Item -Path "bin","obj" -Recurse -Force -ErrorAction SilentlyContinue

# Clean outputs
cd F:\3.Laptrinh\DUANUSB2
Remove-Item -Path "output","installer\bin","installer\obj","deployment" -Recurse -Force -ErrorAction SilentlyContinue

# Full rebuild
.\scripts\build-all.ps1
```

## Next Steps

After successful build:
1. Test MSI installation on clean Windows machine
2. Verify service starts automatically
3. Test DongleCreatorTool creates valid dongles
4. Verify dongle validation works
5. Distribute package to users

## Support

For build issues, check:
- `DongleSyncService` build logs
- `installer\obj\` for WiX compilation errors
- PowerShell script output for detailed error messages

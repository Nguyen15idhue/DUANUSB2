# Building Production Package

## Overview

This guide explains how to build the complete production package for the USB Dongle Sync System with **professional GUI installer**.

## Prerequisites

1. **.NET 8.0 SDK** - For building projects
2. **Inno Setup 6** - For creating professional GUI installer (⭐ RECOMMENDED)
   - Download from: https://jrsoftware.org/isdl.php
   - Install: `innosetup-6.x.x.exe` (Unicode version)
   - Path: `C:\Program Files (x86)\Inno Setup 6\`
3. **PowerShell 5.1+** - For running build scripts

> ⚠️ **Note**: MSI installer (WiX) is deprecated due to service installation issues.

## 🚀 Quick Build (Recommended)

### Build Professional GUI Installer

```powershell
cd F:\3.Laptrinh\DUANUSB2\scripts
.\build-installer.ps1
```

This will:
1. ✅ Check Inno Setup installation
2. 🔨 Build DongleSyncService (Release, self-contained)
3. 📋 Verify all required files
4. 🚀 Compile Inno Setup installer
5. ✅ Create `DongleSyncService-Setup-v1.0.0.exe`

**Output**: `output\DongleSyncService-Setup-v1.0.0.exe` (~60-80 MB)

### Skip Build (Use Existing Binaries)

```powershell
.\build-installer.ps1 -SkipBuild
```

## Step-by-Step Build

If you prefer to run each step individually:

### 1. Build DongleSyncService

```powershell
cd F:\3.Laptrinh\DUANUSB2\src\DongleSyncService
dotnet publish -c Release -r win-x64 --self-contained true
```

**Output**: `bin\Release\net8.0\win-x64\publish\` (self-contained, includes all dependencies)

### 2. Build DongleCreatorTool

```powershell
cd F:\3.Laptrinh\DUANUSB2
.\scripts\publish-dongle-creator.ps1
```

**Output**: `output\DongleCreatorTool\DongleCreatorTool.exe` (standalone, ~60MB)

### 3. Build Inno Setup Installer

```powershell
cd F:\3.Laptrinh\DUANUSB2\scripts
.\build-installer.ps1
```

**Output**: `output\DongleSyncService-Setup-v1.0.0.exe` (GUI installer, ~60-80MB)

## Distribution Package

### For End Users (Recommended)
Send only **one file**:

```
DongleSyncService-Setup-v1.0.0.exe    # Professional GUI installer
```

**Installation Steps**:
1. Right-click → Run as administrator
2. Follow wizard (Next → Next → Install → Finish)
3. Service automatically starts

### Complete Package (Optional)
If you want to include DongleCreatorTool:

```
DongleSyncService-Setup-v1.0.0.exe    # Service installer
DongleCreatorTool.exe                  # USB dongle creator
README.md                              # Quick start guide
```

## What the Installer Does

The Inno Setup installer automatically:

1. ✅ **Detects CHC Geomatics Office 2** - Shows warning if not installed
2. ✅ **Stops old service** - If already running
3. ✅ **Removes old service** - If exists
4. ✅ **Copies files** to `C:\Program Files\CHC Geomatics\Dongle Service\`
5. ✅ **Creates data folders** in `C:\ProgramData\DongleSyncService\`
6. ✅ **Installs Windows Service** - Auto-start, LocalSystem account
7. ✅ **Configures recovery** - Auto-restart on failure
8. ✅ **Starts service** - Immediately
9. ✅ **Creates shortcuts** - Start Menu, Desktop (optional)

## Troubleshooting

### Inno Setup Not Found

If `build-installer.ps1` fails with "Inno Setup not found":

1. Download from: https://jrsoftware.org/isdl.php
2. Install `innosetup-6.x.x.exe` (Unicode version)
3. Default path: `C:\Program Files (x86)\Inno Setup 6\`
4. Retry build

### Service Executable Not Found

If missing `DongleSyncService.exe`:

```powershell
cd F:\3.Laptrinh\DUANUSB2\src\DongleSyncService
dotnet publish -c Release -r win-x64 --self-contained true
```

### DLLPatch.dll Not Found

If missing `DLLPatch.dll`:

```powershell
cd F:\3.Laptrinh\DUANUSB2\src\DLLPatch
dotnet build -c Release
```

Then copy to publish folder:

```powershell
Copy-Item "bin\Release\net8.0\DLLPatch.dll" `
  -Destination "..\DongleSyncService\bin\Release\net8.0\win-x64\publish\" -Force
```

### License File Missing

If `License.rtf` not found:

```powershell
# Use default license
Copy-Item "deployment\DongleSyncSystem-v1.0.0\Installer\License.rtf" `
  -Destination "installer\" -Force
```

### Icon File Missing

If `icon.ico` not found:

- File should be at: `installer\icon.ico`
- Use placeholder or create custom icon
- Rebuild installer

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
.\build-installer.ps1
```

## Version Management

To change version number:

1. **Edit Inno Setup script**: `installer\DongleSyncService-Setup.iss`
   ```iss
   #define MyAppVersion "1.0.1"  ; Change here
   ```

2. **Edit C# project**: `src\DongleSyncService\DongleSyncService.csproj`
   ```xml
   <Version>1.0.1</Version>
   ```

3. **Rebuild installer**:
   ```powershell
   .\build-installer.ps1
   ```

Output will be: `DongleSyncService-Setup-v1.0.1.exe`

## Distribution

### Simple Distribution (Recommended)

Send **one file** to customer:

```
DongleSyncService-Setup-v1.0.0.exe
```

Customer simply:
1. Double-click installer
2. Follow wizard
3. Done!

### Complete Distribution Package

If customer needs dongle creation tool:

```
Package Contents:
├── DongleSyncService-Setup-v1.0.0.exe   # Service installer
├── DongleCreatorTool.exe                 # Dongle creator
└── README.md                             # Quick start guide
```

### For Internal Testing
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

# WiX Toolset v3.11 Installation Guide

## Issue
You have **WiX Toolset v6.0** installed, but this project requires **WiX v3.11** due to incompatibility.

## Why v3.11?
- `Product.wxs` installer definition uses WiX v3.x syntax
- WiX v6.0 introduced breaking changes:
  - Different XML namespace (`http://wixtoolset.org/schemas/v4/wxs` vs `http://schemas.microsoft.com/wix/2006/wi`)
  - Restructured element hierarchy
  - Changed tooling (single `wix.exe` command vs `candle.exe` + `light.exe`)
- Converting requires significant rewrite of the entire installer definition

## Solution: Install WiX v3.11

### Step 1: Download WiX v3.11
Visit the official release page:
```
https://github.com/wixtoolset/wix3/releases/tag/wix3112rtm
```

Download: **wix311.exe** (WiX v3.11.2 Installer)

### Step 2: Install WiX v3.11
1. Run `wix311.exe`
2. Follow the installation wizard
3. Default installation path: `C:\Program Files (x86)\WiX Toolset v3.11\`
4. **Note**: Both v3.11 and v6.0 can coexist on the same system

### Step 3: Verify Installation
Open PowerShell and run:
```powershell
& "C:\Program Files (x86)\WiX Toolset v3.11\bin\candle.exe" -?
```

Expected output: Candle usage information

### Step 4: Build the MSI
Navigate to your project and run:
```powershell
cd F:\3.Laptrinh\DUANUSB2
.\scripts\build-all.ps1
```

Or build just the MSI:
```powershell
.\scripts\build-msi.ps1
```

## Installation Paths
- **WiX v3.11**: `C:\Program Files (x86)\WiX Toolset v3.11\bin\`
- **WiX v6.0**: `C:\Program Files\WiX Toolset v6.0\bin\`

## Build Tools Used
WiX v3.11 includes:
- `candle.exe` - Compiler (WXS → WIXOBJ)
- `light.exe` - Linker (WIXOBJ → MSI)
- Extensions:
  - `WixUtilExtension` - Service installation, file permissions
  - `WixUIExtension` - Installer UI components

## Expected MSI Output
After successful build:
```
F:\3.Laptrinh\DUANUSB2\installer\bin\DongleSyncService-Setup.msi
```

File size: ~8-10 MB (includes .NET 8.0 Service binary)

## Troubleshooting

### "candle.exe not found"
- Verify WiX v3.11 installation at: `C:\Program Files (x86)\WiX Toolset v3.11\`
- Reinstall if necessary

### "The Wix element has an incorrect namespace"
- This error appears when using WiX v6.0 with v3.x WXS files
- Solution: Install WiX v3.11 as described above

### Build warnings about ICE61
- ICE61: "This product should remove only older versions of itself"
- This is a standard warning and can be safely ignored
- Suppressed in build script with `-sice:ICE61`

## Alternative: Upgrade to WiX v6.0 (Advanced)
If you prefer to use WiX v6.0, the following changes are required:

1. **Update namespace** in `Product.wxs`:
   ```xml
   <!-- Old (v3.x) -->
   <Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
   
   <!-- New (v6.0) -->
   <Wix xmlns="http://wixtoolset.org/schemas/v4/wxs">
   ```

2. **Restructure element hierarchy** - v6.0 has different element nesting rules

3. **Update build script** - Replace candle/light with single `wix build` command

4. **Update extension references** - New extension naming convention

**Note**: This requires significant XML restructuring and testing. Using v3.11 is recommended for stability.

## References
- WiX v3.11 Documentation: https://wixtoolset.org/docs/v3/
- WiX v3.11 Releases: https://github.com/wixtoolset/wix3/releases
- Migration Guide (v3 → v6): https://wixtoolset.org/docs/intro/#migrating-from-wix-v3

---

**Quick Command Reference**

Download WiX v3.11:
```powershell
Start-Process "https://github.com/wixtoolset/wix3/releases/tag/wix3112rtm"
```

Verify installation:
```powershell
Test-Path "C:\Program Files (x86)\WiX Toolset v3.11\bin\candle.exe"
```

Build MSI:
```powershell
cd F:\3.Laptrinh\DUANUSB2
.\scripts\build-msi.ps1
```

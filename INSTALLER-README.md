# USB Dongle Sync Service - Quick Start Guide

## For End Users

### System Requirements
- Windows 10/11 (64-bit)
- CHC Geomatics Office 2 (must be installed first)
- Administrator privileges

### Installation Steps

1. **Install CHC Geomatics Office 2** (if not already installed)

2. **Run Installer**
   - Right-click `DongleSyncService-Setup-v1.0.0.exe`
   - Select **"Run as administrator"**
   - Follow installation wizard:
     - Welcome → Next
     - License Agreement → I Accept
     - Destination Folder → Next (or choose custom)
     - Ready to Install → Install
     - Completing Setup → Finish

3. **Service Auto-Starts**
   - The service starts automatically after installation
   - Runs in background, no UI needed

### Using the Service

1. **Insert USB Dongle**
   - Service detects USB automatically
   - Features unlock when valid dongle is present

2. **Remove USB Dongle**
   - Features automatically lock
   - Original state restored

### Verification

Check service status:
```
Services → Find "USB Dongle Sync Service" → Status should be "Running"
```

View logs:
```
C:\ProgramData\DongleSyncService\logs\
```

### Uninstallation

**Option 1**: Control Panel
- Settings → Apps → USB Dongle Sync Service → Uninstall

**Option 2**: Start Menu
- Start Menu → CHC Geomatics → Uninstall USB Dongle Sync Service

### Troubleshooting

**Service not starting?**
- Ensure CHC Geomatics Office 2 is installed
- Check Windows Event Viewer for errors
- View logs in `C:\ProgramData\DongleSyncService\logs\`

**Dongle not detected?**
- Verify USB dongle is properly formatted
- Check dongle contains required files (`dongle.key`, `iv.bin`, `patch.dll.enc`)
- Try different USB port

**Features not working?**
- Verify dongle GUID matches system
- Check service logs for errors
- Restart service: `services.msc` → DongleSyncService → Restart

### Support

For technical support, contact your system administrator or vendor.

---

## For Administrators

### Service Details
- **Name**: DongleSyncService
- **Display Name**: USB Dongle Sync Service
- **Startup Type**: Automatic
- **Account**: LocalSystem
- **Install Path**: `C:\Program Files\CHC Geomatics\Dongle Service\`
- **Data Path**: `C:\ProgramData\DongleSyncService\`

### Service Management

**Start Service**:
```powershell
Start-Service DongleSyncService
```

**Stop Service**:
```powershell
Stop-Service DongleSyncService
```

**View Status**:
```powershell
Get-Service DongleSyncService
```

**View Recent Logs**:
```powershell
Get-Content "C:\ProgramData\DongleSyncService\logs\service-$(Get-Date -Format 'yyyyMMdd').log" -Tail 50
```

### Creating USB Dongles

Use `DongleCreatorTool.exe` (provided separately):

1. Launch DongleCreatorTool
2. Select USB drive
3. Enter binding password (if required)
4. Click "Create Dongle"
5. Wait for completion
6. Test dongle on target system

### Security Notes

- USB dongle contains encrypted patch DLL
- Binding password prevents unauthorized dongle creation
- Service validates dongle GUID and encryption
- Original DLL is backed up before patching
- Automatic restoration on dongle removal

### Architecture

```
USB Dongle (D:\dongle\)
├── dongle.key           # AES encryption key
├── iv.bin               # Initialization vector
└── patch.dll.enc        # Encrypted DLLPatch.dll

DongleSyncService
├── Monitors USB insertion/removal (WMI events)
├── Validates dongle GUID
├── Decrypts patch DLL
├── Backs up original DLL
├── Patches target DLL
├── Monitors integrity (heartbeat)
└── Restores on dongle removal
```

### Files Managed by Service

**Target DLL** (patched when dongle inserted):
```
C:\Users\<USER>\AppData\Roaming\CHCNAV\CHC Geomatics Office 2\CHC.CGO.Common.dll
```

**Backup** (original DLL):
```
C:\ProgramData\DongleSyncService\backups\CHC.CGO.Common.dll.original
C:\ProgramData\DongleSyncService\backups\CHC.CGO.Common.dll.metadata.json
```

**Logs**:
```
C:\ProgramData\DongleSyncService\logs\service-yyyyMMdd.log
```

**State**:
```
C:\ProgramData\DongleSyncService\state.json
```

### Configuration

Service configuration is stored in:
```
C:\Program Files\CHC Geomatics\Dongle Service\appsettings.json
```

Default settings:
- Dongle GUID: `8db3f8a4-8021-4473-868a-a53bb2e39759`
- Target DLL pattern: `CHC.CGO.Common.dll`
- Heartbeat interval: 1000ms (1 second)
- Log retention: 7 days

### Version Information

**Current Version**: 1.0.0

**Release Date**: December 2024

**Changelog**:
- v1.0.0: Initial release
  - USB dongle detection and validation
  - Encrypted DLL patching
  - Automatic backup and restore
  - Heartbeat integrity monitoring
  - Professional GUI installer

---

**© 2024 CHC Geomatics. All rights reserved.**

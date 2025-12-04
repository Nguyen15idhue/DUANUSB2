# Changelog

## [2025-12-04] - Critical Bug Fixes

### Fixed
- **Path Resolution Bug in DLL Operations**: Fixed critical issue where service was using wrong DLL paths from cached state instead of actual installation paths
  - Modified `DLLManager.RestoreDLL()` to read correct path from backup metadata
  - Modified `DLLManager.PatchDLL()` to override cached path with metadata path
  - Modified `HeartbeatMonitor.CheckDLLIntegrity()` to use correct path for integrity checks
  
### Technical Details
- **Root Cause**: AppFinder cached DLL path during development (`F:\3.Laptrinh\DUANUSB2\dllgoc\`) persisted in state file, causing Access Denied errors when service tried to patch/restore files in wrong location
- **Solution**: All DLL operations now prioritize file path from `backup.metadata.json` which contains the correct installed path (`C:\Users\ADMIN\AppData\Roaming\CHCNAV\CHC Geomatics Office 2\`)
- **Impact**: 
  - ✅ DLL patching now works correctly
  - ✅ DLL restoration on USB removal works correctly  
  - ✅ Integrity checks no longer trigger false positives
  - ✅ No more "Access Denied" errors

### Files Modified
- `src/DongleSyncService/Services/DLLManager.cs`
  - Line 337-360: Added metadata path override in `PatchDLL()`
  - Line 131-143: Added metadata path override in `RestoreDLL()`
- `src/DongleSyncService/Services/HeartbeatMonitor.cs`
  - Line 122-150: Added metadata path override in `CheckDLLIntegrity()`

### Testing
- ✅ Full patch → remove USB → restore cycle tested successfully
- ✅ Heartbeat integrity monitoring working correctly
- ✅ No permission issues with file operations

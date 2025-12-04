# 🎯 WINDOWS SERVICE INSTALLATION GUIDE

## 📋 ĐÃ HOÀN THÀNH

✅ **Tính năng mới trong DongleCreatorTool:**
- Tự động xóa `bind.key` sau khi tạo dongle thành công
- Không cần chạy lệnh thủ công nữa!

✅ **Scripts cài đặt Windows Service:**
- `install-service.ps1` - Cài đặt service tự động
- `uninstall-service.ps1` - Gỡ cài đặt service

---

## 🚀 CÁCH SỬ DỤNG

### Bước 1: Build Release
```powershell
cd F:\3.Laptrinh\DUANUSB2\src
dotnet build -c Release
```

### Bước 2: Install Service (Run as Administrator)
```powershell
# Right-click PowerShell → Run as Administrator
cd F:\3.Laptrinh\DUANUSB2\scripts
.\install-service.ps1
```

Hoặc tự động tìm binary:
```powershell
.\install-service.ps1 -BinaryPath "F:\3.Laptrinh\DUANUSB2\src\DongleSyncService\bin\Release\net8.0\DongleSyncService.exe"
```

### Bước 3: Verify Service
```powershell
# Check service status
Get-Service DongleSyncService

# View logs
Get-Content C:\ProgramData\DongleSyncService\logs\service-*.log -Tail 50
```

### Bước 4: Tạo USB Dongle
```powershell
# Run DongleCreatorTool
Start-Process "F:\3.Laptrinh\DUANUSB2\src\DongleCreatorTool\bin\Release\net8.0-windows\DongleCreatorTool.exe"

# Trong UI:
# 1. Select USB D:
# 2. Browse DLL
# 3. Click "Create Dongle"
# 4. ✅ bind.key tự động xóa!
```

### Bước 5: Test
```powershell
# Cắm USB → Service tự động detect và patch
# Rút USB → Service tự động restore

# View logs real-time
Get-Content C:\ProgramData\DongleSyncService\logs\service-*.log -Wait
```

---

## 🗑️ GỠ CÀI ĐẶT

```powershell
# Run as Administrator
cd F:\3.Laptrinh\DUANUSB2\scripts
.\uninstall-service.ps1

# Sẽ hỏi có xóa data không (y/n)
```

---

## 📂 SERVICE FILES

### Binary Location (after install)
```
F:\3.Laptrinh\DUANUSB2\src\DongleSyncService\bin\Release\net8.0\DongleSyncService.exe
```

### Data Directory
```
C:\ProgramData\DongleSyncService\
├── logs\           (Service logs)
├── backups\        (DLL backups)
├── state.json      (Current state)
└── bind.key        (Machine binding)
```

### Service Configuration
- **Name**: `DongleSyncService`
- **Display Name**: `USB Dongle Sync Service`
- **Start Type**: Automatic
- **Account**: LocalSystem
- **Description**: Manages USB dongle authentication and DLL patching for CHC Geomatics Office 2

---

## 🎛️ QUẢN LÝ SERVICE

### Start/Stop Service
```powershell
# Start
Start-Service DongleSyncService

# Stop
Stop-Service DongleSyncService

# Restart
Restart-Service DongleSyncService
```

### Check Status
```powershell
Get-Service DongleSyncService | Format-List *
```

### View Logs
```powershell
# Last 50 lines
Get-Content C:\ProgramData\DongleSyncService\logs\service-*.log -Tail 50

# Real-time
Get-Content C:\ProgramData\DongleSyncService\logs\service-*.log -Wait

# Filter errors only
Get-Content C:\ProgramData\DongleSyncService\logs\service-*.log | Select-String "ERR"
```

### Check Windows Event Viewer
```powershell
Get-EventLog -LogName Application -Source DongleSyncService -Newest 10
```

---

## 🔧 TROUBLESHOOTING

### Service won't start
```powershell
# Check if binary exists
Test-Path "F:\3.Laptrinh\DUANUSB2\src\DongleSyncService\bin\Release\net8.0\DongleSyncService.exe"

# Check permissions
icacls "C:\ProgramData\DongleSyncService"

# View recent errors
Get-Content C:\ProgramData\DongleSyncService\logs\service-*.log -Tail 100 | Select-String "ERR|FATAL"
```

### Service crashes on USB insert
```powershell
# Check state file
Get-Content C:\ProgramData\DongleSyncService\state.json | ConvertFrom-Json | Format-List

# Check binding
Get-Content C:\ProgramData\DongleSyncService\bind.key | ConvertFrom-Json | Format-List

# Reset state
Stop-Service DongleSyncService
Remove-Item C:\ProgramData\DongleSyncService\state.json
Start-Service DongleSyncService
```

### DLL restore fails
```powershell
# Check if application is running
Get-Process | Where-Object {$_.Modules.FileName -like "*CHC.CGO.Common.dll*"}

# Force restore from backup
$backup = Get-ChildItem C:\ProgramData\DongleSyncService\backups\*.bak | Sort-Object LastWriteTime -Descending | Select-Object -First 1
Copy-Item $backup.FullName -Destination "C:\Users\ADMIN\AppData\Roaming\CHCNAV\CHC Geomatics Office 2\CHC.CGO.Common.dll" -Force
```

---

## 🎯 TESTING CHECKLIST

- [ ] Service starts automatically on Windows boot
- [ ] USB insert detected within 2 seconds
- [ ] Dongle validation passes
- [ ] DLL patched successfully
- [ ] Application runs with patched DLL
- [ ] USB remove detected within 5 seconds (heartbeat)
- [ ] DLL restored successfully
- [ ] Re-plug USB → hardware key stable (no "Hardware ID mismatch")
- [ ] Service survives system restart while USB plugged
- [ ] Logs are written correctly
- [ ] bind.key auto-deleted when creating new dongle

---

## 📝 NOTES

1. **Auto-delete bind.key**: DongleCreatorTool bây giờ tự động xóa `bind.key` sau khi tạo dongle. Không cần chạy lệnh thủ công nữa!

2. **Service vs Console Mode**:
   - **Service mode** (production): Chạy nền, tự động start, logs vào file
   - **Console mode** (dev): `dotnet run` để debug, logs ra console

3. **Permissions**: Service chạy với LocalSystem account nên có full permissions để patch DLLs

4. **Recovery**: Service tự động restart nếu crash (configured trong install script)

5. **Logs Rotation**: Serilog tự động tạo file mới mỗi ngày (`service-20251204.log`)

---

## 🔐 SECURITY

- Service chạy với LocalSystem → có quyền cao
- bind.key chứa machine fingerprint → chỉ máy này dùng được dongle
- Backups được lưu trong `C:\ProgramData` → chỉ admin access
- Logs không chứa sensitive data (passwords, keys)

---

## 📞 SUPPORT

Nếu có vấn đề:
1. Check logs: `C:\ProgramData\DongleSyncService\logs\`
2. Check Windows Event Viewer
3. Verify service status: `Get-Service DongleSyncService`
4. Check file permissions on `C:\ProgramData\DongleSyncService\`
5. Review documentation: `docs/DEV-TESTING-COMMANDS.md`

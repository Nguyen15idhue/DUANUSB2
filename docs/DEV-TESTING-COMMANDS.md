# 🛠️ DEVELOPER TESTING COMMANDS

> **⚠️ CHỈ DÀNH CHO DEVELOPER**  
> Các lệnh này dùng để test và debug trong quá trình phát triển.  
> **NGƯỜI DÙNG CUỐI KHÔNG BAO GIỜ CẦN CHẠY** - họ chỉ cài MSI installer.

---

## 📋 MỤC LỤC
- [Build Projects](#build-projects)
- [Test E2E Flow](#test-e2e-flow)
- [Fix USB Hardware ID Bug](#fix-usb-hardware-id-bug)
- [Clean & Reset State](#clean--reset-state)
- [Troubleshooting](#troubleshooting)

---

## 🔨 BUILD PROJECTS

### Build tất cả projects
```powershell
cd F:\3.Laptrinh\DUANUSB2\src
dotnet build
```

### Build từng project riêng
```powershell
# DongleSyncService
cd F:\3.Laptrinh\DUANUSB2\src\DongleSyncService
dotnet build

# DongleCreatorTool
cd F:\3.Laptrinh\DUANUSB2\src\DongleCreatorTool
dotnet build

# DLLPatch
cd F:\3.Laptrinh\DUANUSB2\src\DLLPatch
dotnet build
```

### Build Release mode
```powershell
cd F:\3.Laptrinh\DUANUSB2\src
dotnet build -c Release
```

---

## 🧪 TEST E2E FLOW

### 1. Chạy DongleSyncService trong console mode
```powershell
# Stop service nếu đang chạy
Stop-Process -Name DongleSyncService -Force -ErrorAction SilentlyContinue

# Chạy service trong console để xem log
cd F:\3.Laptrinh\DUANUSB2\src\DongleSyncService
dotnet run
```

### 2. Tạo USB Dongle
```powershell
# Chạy DongleCreatorTool
Start-Process "F:\3.Laptrinh\DUANUSB2\src\DongleCreatorTool\bin\Debug\net8.0-windows\DongleCreatorTool.exe"

# Trong UI:
# - Select USB drive: D:
# - Browse DLL: C:\Users\ADMIN\AppData\Roaming\CHCNAV\CHC Geomatics Office 2\CHC.CGO.Common.dll
# - Click "Create Dongle"
```

### 3. Verify Dongle Structure
```powershell
# Kiểm tra files trên USB
Get-ChildItem D:\dongle

# Expected output:
# config.json
# dongle.key
# patch.dll.enc
# iv.bin
# README.txt
```

### 4. Test USB Insert/Remove Cycle
```powershell
# Cắm USB → quan sát log service:
# - USB inserted detected
# - Dongle validation passed
# - Machine binding created/validated
# - DLL patched successfully

# Rút USB → quan sát log:
# - Heartbeat failed or USB removed
# - DLL restored successfully
```

### 5. Test Re-plug (Hardware ID Stability)
```powershell
# Rút USB
# Đợi 5 giây
# Cắm lại USB
# Kiểm tra log: Hardware ID phải GIỐNG NHAU (không có "Hardware ID mismatch")
```

---

## 🐛 FIX USB HARDWARE ID BUG

> **Context:** VolumeSerialNumber thay đổi khi USB re-plug, gây lỗi validation.  
> **Fix:** Dùng PNPDeviceID thay vì VolumeSerialNumber.

### Full Rebuild Workflow
```powershell
# Step 1: Stop all running processes
Stop-Process -Name DongleSyncService -Force -ErrorAction SilentlyContinue
Stop-Process -Name DongleCreatorTool -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

# Step 2: Build both projects
cd F:\3.Laptrinh\DUANUSB2\src\DongleSyncService
dotnet build

cd F:\3.Laptrinh\DUANUSB2\src\DongleCreatorTool
dotnet build

# Step 3: Delete old dongle.key (chứa hardware key cũ)
Remove-Item D:\dongle\dongle.key -Force -ErrorAction SilentlyContinue
Write-Host "✅ Deleted old dongle.key"

# Step 4: Recreate dongle with NEW hardware key
Start-Process "F:\3.Laptrinh\DUANUSB2\src\DongleCreatorTool\bin\Debug\net8.0-windows\DongleCreatorTool.exe"
Write-Host "📝 Select USB D:, browse DLL, click Create"
Write-Host "⏳ Waiting for dongle creation..."
Read-Host "Press Enter when dongle created"

# Step 5: Delete old binding file
Remove-Item C:\ProgramData\DongleSyncService\bind.key -Force -ErrorAction SilentlyContinue
Write-Host "✅ Deleted old bind.key"

# Step 6: Run updated service
cd F:\3.Laptrinh\DUANUSB2\src\DongleSyncService
Write-Host "🚀 Starting service with fixed USB validation..."
dotnet run

# Step 7: Test re-plug
Write-Host "`n📋 Test Steps:"
Write-Host "1. Wait for USB detected"
Write-Host "2. Unplug USB"
Write-Host "3. Wait 5 seconds"
Write-Host "4. Re-plug USB"
Write-Host "5. Check log: Hardware ID should match!"
```

---

## 🧹 CLEAN & RESET STATE

### Xóa tất cả state files
```powershell
# Xóa state directory (backup sẽ được giữ)
Remove-Item C:\ProgramData\DongleSyncService\state.json -Force -ErrorAction SilentlyContinue
Remove-Item C:\ProgramData\DongleSyncService\bind.key -Force -ErrorAction SilentlyContinue
Write-Host "✅ State files deleted"
```

### Xóa dongle từ USB
```powershell
# Xóa toàn bộ dongle folder
Remove-Item D:\dongle -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "✅ Dongle folder deleted from USB"
```

### Restore DLL thủ công (nếu cần)
```powershell
# List backups
Get-ChildItem C:\ProgramData\DongleSyncService\backups\

# Restore từ backup mới nhất
$latestBackup = Get-ChildItem C:\ProgramData\DongleSyncService\backups\*.bak | Sort-Object LastWriteTime -Descending | Select-Object -First 1
$targetDLL = "C:\Users\ADMIN\AppData\Roaming\CHCNAV\CHC Geomatics Office 2\CHC.CGO.Common.dll"

Copy-Item $latestBackup.FullName -Destination $targetDLL -Force
Write-Host "✅ DLL restored from: $($latestBackup.Name)"
```

---

## 🔍 TROUBLESHOOTING

### Lỗi: Service đang chạy, không build được
```powershell
# Find và kill process
Get-Process DongleSyncService -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 1
dotnet build
```

### Lỗi: DongleCreatorTool không đóng được
```powershell
# Kill by PID (xem trong error message)
Stop-Process -Id <PID> -Force

# Hoặc kill tất cả
Get-Process DongleCreatorTool -ErrorAction SilentlyContinue | Stop-Process -Force
```

### Lỗi: USB Hardware ID mismatch
```powershell
# Check hardware key trong dongle
Get-Content D:\dongle\dongle.key

# Check binding
Get-Content C:\ProgramData\DongleSyncService\bind.key | ConvertFrom-Json | Format-List

# Solution: Xóa cả 2 file và tạo lại
Remove-Item D:\dongle\dongle.key -Force
Remove-Item C:\ProgramData\DongleSyncService\bind.key -Force
# Sau đó recreate dongle và re-plug USB
```

### Lỗi: DLL restore failed (file locked)
```powershell
# Check process đang lock file
$dllPath = "C:\Users\ADMIN\AppData\Roaming\CHCNAV\CHC Geomatics Office 2\CHC.CGO.Common.dll"

# Tìm process
$processes = Get-Process | Where-Object { 
    try { $_.Modules.FileName -contains $dllPath } catch {} 
}
$processes | Format-Table Name, Id, Path

# Kill application
Stop-Process -Name "CHC.Geomatics.Office.2" -Force -ErrorAction SilentlyContinue

# Retry restore
# (Service sẽ tự retry khi heartbeat fail)
```

### Check logs
```powershell
# Service logs (nếu chạy console mode, logs hiển thị trực tiếp)
# Nếu chạy như service:
Get-Content C:\ProgramData\DongleSyncService\logs\*.log -Tail 50

# Check state file
Get-Content C:\ProgramData\DongleSyncService\state.json | ConvertFrom-Json | Format-List
```

---

## 📊 VERIFY FIX SUCCESS

### Test Hardware ID Stability
```powershell
# Script tự động test re-plug
$service = Start-Process -FilePath "dotnet" -ArgumentList "run" -WorkingDirectory "F:\3.Laptrinh\DUANUSB2\src\DongleSyncService" -PassThru -NoNewWindow

Write-Host "⏳ Waiting for service startup (5s)..."
Start-Sleep -Seconds 5

Write-Host "📋 Manual test required:"
Write-Host "1. Cắm USB D:"
Read-Host "Press Enter when USB detected in log"

Write-Host "2. Rút USB"
Read-Host "Press Enter when USB removed"

Write-Host "3. Đợi 5 giây..."
Start-Sleep -Seconds 5

Write-Host "4. Cắm lại USB D:"
Read-Host "Press Enter when USB re-plugged"

Write-Host "`n✅ Check log for:"
Write-Host "   - No 'Hardware ID mismatch' error"
Write-Host "   - USB validated successfully"
Write-Host "   - DLL patched again"

Read-Host "Press Enter to stop service"
Stop-Process -Id $service.Id -Force
```

---

## 📦 RELEASE BUILD

### Build cho production
```powershell
# Clean old builds
cd F:\3.Laptrinh\DUANUSB2\src
dotnet clean

# Build Release
dotnet build -c Release

# Verify binaries
Get-ChildItem F:\3.Laptrinh\DUANUSB2\src\DongleSyncService\bin\Release\net8.0\
Get-ChildItem F:\3.Laptrinh\DUANUSB2\src\DongleCreatorTool\bin\Release\net8.0-windows\
```

### Test trên clean environment
```powershell
# Tạo test VM hoặc clean machine
# Copy binaries
# Run installer (sau khi tạo MSI)
# Verify E2E flow
```

---

## 🎯 QUICK REFERENCE

### One-liner: Rebuild Everything
```powershell
Stop-Process -Name DongleSyncService,DongleCreatorTool -Force -ErrorAction SilentlyContinue; Start-Sleep -Seconds 1; cd F:\3.Laptrinh\DUANUSB2\src; dotnet build; Write-Host "✅ Build complete"
```

### One-liner: Clean State & Recreate
```powershell
Remove-Item C:\ProgramData\DongleSyncService\bind.key,D:\dongle\dongle.key -Force -ErrorAction SilentlyContinue; Write-Host "✅ Ready to recreate dongle"
```

### One-liner: Start Testing
```powershell
cd F:\3.Laptrinh\DUANUSB2\src\DongleSyncService; dotnet run
```

---

## ⚠️ IMPORTANT NOTES

1. **KHÔNG BAO GIỜ** chạy những lệnh này trên production environment
2. **LUÔN LUÔN** stop service trước khi rebuild
3. **KIỂM TRA** USB drive letter trước khi xóa files (đừng nhầm ổ khác!)
4. **BACKUP** DLL gốc trước khi test (service tự backup nhưng double-check)
5. **TEST** re-plug nhiều lần để verify stability

---

## 📞 SUPPORT

Nếu gặp lỗi không có trong troubleshooting:
1. Check service logs
2. Check Windows Event Viewer
3. Verify file permissions trên `C:\ProgramData\DongleSyncService`
4. Test với USB khác để loại trừ hardware issue
5. Review code changes trong USBValidator.cs và USBWriter.cs

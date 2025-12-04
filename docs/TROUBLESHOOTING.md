# Xử Lý Sự Cố - Dongle Sync Service

## 🚨 Các Kịch Bản Không Mong Muốn & Cách Xử Lý

---

## 1️⃣ LỖI CÀI ĐẶT

### Kịch Bản 1.1: Windows SmartScreen Chặn Installer
**Triệu chứng:**
- Windows hiện: "Windows protected your PC"
- Không cho chạy installer

**Nguyên nhân:**
- File EXE chưa có digital signature
- Windows 11 mặc định chặn file không rõ nguồn

**Cách xử lý:**
1. Click **"More info"**
2. Click **"Run anyway"**
3. Hoặc: Chuột phải file → Properties → Tab "General" → Tick "Unblock" → Apply

### Kịch Bản 1.2: Antivirus Xóa/Cách Ly File
**Triệu chứng:**
- Installer biến mất sau khi tải
- Windows Defender báo "Threat detected"
- Avast/Norton/Kaspersky cách ly file

**Nguyên nhân:**
- DLL patching bị nhận diện là hành vi "injection"
- File chưa có certificate

**Cách xử lý:**
1. **Tắt tạm Antivirus:**
   - Windows Defender: Settings → Windows Security → Virus & threat protection → Manage settings → Real-time protection = OFF
   - Avast/Norton: Tạm disable 10 phút

2. **Cài đặt installer**

3. **Add exception (Quan trọng!):**
   - Windows Defender: Add exclusion → Folder:
     - `C:\Program Files\CHC Geomatics\Dongle Service\`
     - `C:\ProgramData\DongleSyncService\`
   - Avast/Norton: Settings → Exceptions → Add:
     - `DongleSyncService.exe`
     - `DLLPatch.dll`

4. **Bật lại Antivirus**

### Kịch Bản 1.3: Installer Báo "CHC Geomatics Office 2 Not Found"
**Triệu chứng:**
- Cài đặt thất bại
- Message: "Please install CHC Geomatics Office 2 first"

**Nguyên nhân:**
- CHC chưa cài đặt **HOẶC**
- CHC đã cài nhưng **chưa chạy lần nào** (chưa có DLL copy trong AppData)

**Lưu ý:** Installer không check CHC exe, mà check file `CHC.CGO.Common.dll` trong user profile!

**Cách xử lý:**

**1. Kiểm tra DLL có tồn tại không:**
```powershell
# Installer check 3 đường dẫn này:
Test-Path "$env:LOCALAPPDATA\CHCNAV\CHC Geomatics Office 2\CHC.CGO.Common.dll"
Test-Path "$env:APPDATA\CHCNAV\CHC Geomatics Office 2\CHC.CGO.Common.dll"
Test-Path "C:\Program Files\CHCNAV\CHC Geomatics Office 2\CHC.CGO.Common.dll"
```

**2. Nếu CHC đã cài nhưng chưa có DLL:**
- Mở CHC Geomatics Office 2 **một lần**
- CHC sẽ tự động copy DLL vào `AppData\Roaming\CHCNAV\`
- Đóng CHC
- Chạy lại installer

**3. Nếu vẫn không detect:**
- CHC chưa cài đặt → Cài CHC trước
- Hoặc: Copy thủ công DLL từ CHC install folder vào:
  ```
  C:\Users\%USERNAME%\AppData\Roaming\CHCNAV\CHC Geomatics Office 2\
  ```

### Kịch Bản 1.4: "Access Denied" Khi Cài Đặt
**Triệu chứng:**
- Installer fail với lỗi quyền
- Không tạo được folder/file

**Nguyên nhân:**
- Không chạy "Run as administrator"
- Account không có quyền Admin

**Cách xử lý:**
1. Chuột phải installer → **Run as administrator**
2. Nếu vẫn lỗi: Đăng nhập account Administrator thật sự
3. UAC quá cao: Control Panel → User Accounts → Change UAC settings → Kéo xuống 1 bậc

---

## 2️⃣ LỖI SERVICE KHÔNG CHẠY

### Kịch Bản 2.1: Service Status = "Stopped" Sau Cài Đặt
**Triệu chứng:**
- Cài xong nhưng service không chạy
- Services.msc hiện status "Stopped"

**Cách xử lý:**
```powershell
# PowerShell Admin
Start-Service DongleSyncService
Get-Service DongleSyncService

# Nếu không start được, check log:
Get-Content "C:\ProgramData\DongleSyncService\logs\service-$(Get-Date -Format 'yyyyMMdd')*.log" -Tail 50
```

**Lỗi phổ biến trong log:**
- `FileNotFoundException`: Thiếu DLL dependency → Cài lại installer
- `UnauthorizedAccessException`: Quyền không đủ → Chạy lệnh với Admin
- `BindingFailure`: Thiếu .NET 8 Runtime → Không nên xảy ra (installer self-contained)

### Kịch Bản 2.2: Service Start Rồi Ngay Lập Tức Stop
**Triệu chứng:**
- Service start được
- 1-2 giây sau tự stop
- Event Viewer có lỗi

**Cách xử lý:**
```powershell
# Check Event Viewer
Get-EventLog -LogName Application -Source "DongleSyncService" -Newest 10 | Format-List

# Hoặc xem Windows Event Viewer GUI
eventvwr.msc
# → Windows Logs → Application → Tìm source "DongleSyncService"
```

**Lỗi thường gặp:**
- `Access denied to C:\ProgramData\`: Quyền folder sai
- `Port already in use`: IPC pipe name conflict (rất hiếm)

**Fix:**
```powershell
# Fix quyền folder
icacls "C:\ProgramData\DongleSyncService" /grant "NT AUTHORITY\SYSTEM:(OI)(CI)F" /T
icacls "C:\Program Files\CHC Geomatics\Dongle Service" /grant "NT AUTHORITY\SYSTEM:(OI)(CI)F" /T
```

### Kịch Bản 2.3: Service Chạy Nhưng Không Detect USB
**Triệu chứng:**
- Service status = "Running"
- Cắm USB không có phản ứng
- Log không có "USB detected"

**Cách xử lý:**
```powershell
# Xem log realtime
Get-Content "C:\ProgramData\DongleSyncService\logs\service-$(Get-Date -Format 'yyyyMMdd')*.log" -Wait -Tail 20
# Cắm USB → Xem có log không
```

**Nguyên nhân & Fix:**

**A. USB bị ẩn/không mount:**
```powershell
# List tất cả USB
Get-Volume | Where-Object DriveType -eq 'Removable'
Get-PSDrive -PSProvider FileSystem | Where-Object Root -match '^[A-Z]:\\'
```
→ Nếu không thấy USB: Thử cổng USB khác, restart máy

**B. USB không có folder `dongle\`:**
```powershell
# Check structure
Get-ChildItem D:\dongle\  # Thay D: bằng drive letter thật
```
→ Phải có 3 files: `patch.dll.enc`, `iv.bin`, `dongle.key`

**C. WMI Service không chạy (rất hiếm):**
```powershell
Get-Service Winmgmt
Start-Service Winmgmt
```

---

## 3️⃣ LỖI PATCH DLL

### Kịch Bản 3.1: "Access Denied" Khi Patch
**Triệu chứng:**
- Log: `[ERR] Failed to patch DLL`
- Log: `System.UnauthorizedAccessException: Access to the path '...\CHC.CGO.Common.dll' is denied`

**Nguyên nhân:**
- DLL đang bị process CHC giữ (đang mở CHC)
- File có thuộc tính ReadOnly
- Antivirus chặn

**Cách xử lý:**

**1. Đóng tất cả CHC processes:**
```powershell
Get-Process | Where-Object ProcessName -like "*CHC*" | Stop-Process -Force
```

**2. Remove ReadOnly:**
```powershell
$dllPath = "C:\Users\$env:USERNAME\AppData\Roaming\CHCNAV\CHC Geomatics Office 2\CHC.CGO.Common.dll"
Set-ItemProperty -Path $dllPath -Name IsReadOnly -Value $false
Get-Item $dllPath | Select-Object Name, IsReadOnly, Attributes
```

**3. Restart service:**
```powershell
Restart-Service DongleSyncService
```

**4. Cắm lại USB**

### Kịch Bản 3.2: Patch Thành Công Nhưng CHC Vẫn Báo Lỗi License
**Triệu chứng:**
- Log: `[INF] DLL patched successfully`
- Mở CHC vẫn báo "License error"

**Cách xử lý:**

**1. Verify DLL đã patch:**
```powershell
$dllPath = "C:\Users\$env:USERNAME\AppData\Roaming\CHCNAV\CHC Geomatics Office 2\CHC.CGO.Common.dll"
Get-Item $dllPath | Select-Object Length, LastWriteTime

# Length phải là 293,888 bytes (286 KB)
# LastWriteTime phải vừa thay đổi
```

**2. Check dongle files hợp lệ:**
```powershell
# Verify 3 files trên USB
$donglePath = "D:\dongle"  # Thay D: cho đúng
Get-ChildItem $donglePath | Select-Object Name, Length

# patch.dll.enc: ~286 KB
# iv.bin: 16 bytes
# dongle.key: 32 bytes
```

**3. Check binding (nếu có):**
- Nếu dongle được bind với máy cụ thể (Machine Fingerprint)
- Chỉ chạy được trên máy đó
- → Tạo dongle mới không bind, hoặc bind đúng máy

### Kịch Bản 3.3: DLL Bị Restore Ngay Sau Khi Patch
**Triệu chứng:**
- Patch thành công
- Vài giây sau DLL về lại file gốc (287 KB)
- Log có cả "patched" và "restored" liên tục

**Nguyên nhân:**
- CHC có watchdog tự restore DLL
- Antivirus quarantine rồi restore
- Conflict với CHC auto-update

**Cách xử lý:**
1. **Disable CHC auto-update** (nếu có option)
2. **Add antivirus exception** (xem Kịch Bản 1.2)
3. **Check CHC không có service tự bảo vệ:**
```powershell
Get-Service | Where-Object DisplayName -like "*CHC*"
# Nếu có service CHC khác đang chạy → Stop nó
```

---

## 4️⃣ LỖI RESTORE DLL

### Kịch Bản 4.1: Rút USB Nhưng DLL Không Restore
**Triệu chứng:**
- Rút USB
- DLL vẫn là file patched (286 KB)
- CHC vẫn chạy được (không đúng!)

**Nguyên nhân:**
- Service không detect USB removal
- Backup file bị mất
- Service bị crash/hang

**Cách xử lý:**

**1. Check service còn chạy không:**
```powershell
Get-Service DongleSyncService
Get-Process DongleSyncService -ErrorAction SilentlyContinue
```

**2. Check backup tồn tại:**
```powershell
$backupPath = "C:\ProgramData\DongleSyncService\backups\CHC.CGO.Common.dll.original"
Test-Path $backupPath
Get-Item $backupPath | Select-Object Length
# Phải là 294,400 bytes (287 KB)
```

**3. Restore thủ công:**
```powershell
$dllPath = "C:\Users\$env:USERNAME\AppData\Roaming\CHCNAV\CHC Geomatics Office 2\CHC.CGO.Common.dll"
$backupPath = "C:\ProgramData\DongleSyncService\backups\CHC.CGO.Common.dll.original"

# Đóng CHC trước
Get-Process | Where-Object ProcessName -like "*CHC*" | Stop-Process -Force

# Restore
Copy-Item $backupPath -Destination $dllPath -Force
Get-Item $dllPath | Select-Object Length  # Phải 294,400 bytes
```

**4. Restart service:**
```powershell
Restart-Service DongleSyncService
```

### Kịch Bản 4.2: "Backup File Corrupted"
**Triệu chứng:**
- Log: `[ERR] Backup file corrupted or invalid`
- Không restore được

**Nguyên nhân:**
- Backup metadata không khớp
- File backup bị sửa/hỏng

**Cách xử lý:**

**1. Xóa backup cũ:**
```powershell
Remove-Item "C:\ProgramData\DongleSyncService\backups\*" -Force
```

**2. Copy lại DLL gốc từ CHC installer:**
- Tìm file CHC installer gốc
- Extract/cài lại CHC
- Lấy file `CHC.CGO.Common.dll` gốc (287 KB)
- Copy vào: `C:\Users\$env:USERNAME\AppData\Roaming\CHCNAV\CHC Geomatics Office 2\`

**3. Cắm USB lại để tạo backup mới**

---

## 5️⃣ LỖI USER PROFILE

### Kịch Bản 5.1: Multi-User - Service Chỉ Hoạt Động Cho 1 User
**Triệu chứng:**
- User A cài service → OK
- User B login → Không hoạt động

**Nguyên nhân:**
- DLL path dùng `$env:USERNAME` (hardcoded user)
- Service chỉ monitor DLL của user cài đặt

**Cách xử lý:**
```powershell
# Check DLL path trong state.json
Get-Content "C:\ProgramData\DongleSyncService\state.json" | ConvertFrom-Json | Select-Object dllPath

# Path phải dạng:
# C:\Users\{CURRENT_USER}\AppData\Roaming\CHCNAV\CHC Geomatics Office 2\CHC.CGO.Common.dll
```

**Fix:**
1. **Xóa state.json cũ:**
```powershell
Remove-Item "C:\ProgramData\DongleSyncService\state.json" -Force
```

2. **Restart service:**
```powershell
Restart-Service DongleSyncService
```

3. **Login bằng User B → Cắm USB lần đầu**
   - Service tự detect đúng user profile

**Lưu ý:** Mỗi user cần cắm USB lần đầu để service detect đúng path

### Kịch Bản 5.2: Roaming Profile / OneDrive Sync Conflict
**Triệu chứng:**
- DLL path nằm trong OneDrive folder
- OneDrive đang sync → File locked
- Patch/restore thất bại

**Cách xử lý:**

**1. Pause OneDrive sync:**
- System tray → OneDrive icon → More → Pause syncing

**2. Move CHC config ra ngoài OneDrive:**
- CHC settings → Change data folder location
- Chọn folder không sync (C:\ProgramData\CHC\)

**3. Resume OneDrive**

---

## 6️⃣ LỖI WINDOWS UPDATE

### Kịch Bản 6.1: Sau Windows Update Service Bị Disable
**Triệu chứng:**
- Windows update xong
- Service status = "Disabled"

**Cách xử lý:**
```powershell
# Set lại Automatic
Set-Service DongleSyncService -StartupType Automatic
Start-Service DongleSyncService
```

### Kịch Bản 6.2: Windows Update Thay Đổi Permissions
**Triệu chứng:**
- Sau update service không truy cập được files

**Cách xử lý:**
```powershell
# Reset permissions
icacls "C:\ProgramData\DongleSyncService" /reset /T
icacls "C:\Program Files\CHC Geomatics\Dongle Service" /reset /T

# Grant lại
icacls "C:\ProgramData\DongleSyncService" /grant "NT AUTHORITY\SYSTEM:(OI)(CI)F" /T
icacls "C:\Program Files\CHC Geomatics\Dongle Service" /grant "NT AUTHORITY\SYSTEM:(OI)(CI)F" /T

Restart-Service DongleSyncService
```

---

## 7️⃣ LỖI PHẦN CỨNG

### Kịch Bản 7.1: USB Bị Hỏng/Lỗi
**Triệu chứng:**
- USB nhận dạng rồi mất
- Windows "USB device not recognized"
- Files trên USB bị lỗi

**Cách xử lý:**
1. **Test USB trên máy khác**
2. **Format USB (FAT32)** và tạo lại dongle
3. **Thay USB khác**

### Kịch Bản 7.2: USB Hub Không Đủ Power
**Triệu chứng:**
- USB thỉnh thoảng disconnect
- Log: USB detected → USB removed liên tục

**Cách xử lý:**
- Cắm trực tiếp vào cổng USB của máy tính (không qua hub)
- Dùng USB hub có nguồn phụ (powered hub)

---

## 8️⃣ LỖI PERFORMANCE

### Kịch Bản 8.1: Service Ăn CPU/RAM Cao
**Triệu chứng:**
- Task Manager: DongleSyncService.exe dùng >10% CPU
- RAM >100 MB (bình thường ~20-30 MB)

**Nguyên nhân:**
- Heartbeat check quá nhanh (1 giây)
- USB bị disconnect liên tục

**Cách xử lý:**
```powershell
# Check log có lặp không
Get-Content "C:\ProgramData\DongleSyncService\logs\service-$(Get-Date -Format 'yyyyMMdd')*.log" -Tail 100

# Nếu lặp "Heartbeat check failed" → USB không ổn định
# Fix: Thay USB/cổng USB
```

### Kịch Bản 8.2: Log Files Chiếm Dung Lượng Lớn
**Triệu chứng:**
- `C:\ProgramData\DongleSyncService\logs\` >1 GB

**Cách xử lý:**
```powershell
# Xóa log cũ (giữ 7 ngày gần nhất)
$logPath = "C:\ProgramData\DongleSyncService\logs"
Get-ChildItem $logPath -Filter "service-*.log" | 
    Where-Object LastWriteTime -lt (Get-Date).AddDays(-7) | 
    Remove-Item -Force

# Hoặc xóa tất cả log cũ
Remove-Item "$logPath\service-*.log" -Force
```

---

## 9️⃣ LỖI UNINSTALL

### Kịch Bản 9.1: Uninstall Không Xóa Hết Files
**Triệu chứng:**
- Uninstall xong
- Vẫn còn folders: `C:\ProgramData\DongleSyncService\`, service registry

**Cách xử lý:**
```powershell
# 1. Stop và xóa service
Stop-Service DongleSyncService -ErrorAction SilentlyContinue
sc.exe delete DongleSyncService

# 2. Xóa folders
Remove-Item "C:\Program Files\CHC Geomatics\Dongle Service" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "C:\ProgramData\DongleSyncService" -Recurse -Force -ErrorAction SilentlyContinue

# 3. Xóa Start Menu shortcuts
Remove-Item "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\USB Dongle Sync Service" -Recurse -Force -ErrorAction SilentlyContinue
```

---

## 🆘 CHECKLIST XỬ LÝ NHANH

Khi gặp lỗi, làm theo thứ tự:

### Level 1: Cơ Bản
- [ ] Service đang chạy? → `Get-Service DongleSyncService`
- [ ] USB có folder `dongle\` với 3 files?
- [ ] CHC đã cài đặt đúng?
- [ ] Chạy với quyền Admin?

### Level 2: Log & Files
- [ ] Check log: `notepad C:\ProgramData\DongleSyncService\logs\service-YYYYMMDD*.log`
- [ ] DLL file size đúng? (286 KB patched / 287 KB gốc)
- [ ] Backup file tồn tại? `C:\ProgramData\DongleSyncService\backups\`

### Level 3: Quyền & Process
- [ ] DLL không ReadOnly?
- [ ] CHC process đã đóng? → `Get-Process | Where-Object ProcessName -like "*CHC*"`
- [ ] Antivirus có chặn không?
- [ ] Permissions đúng? → Xem Kịch Bản 2.2

### Level 4: Nuclear Option
- [ ] Restart service: `Restart-Service DongleSyncService`
- [ ] Xóa state.json: `Remove-Item C:\ProgramData\DongleSyncService\state.json -Force`
- [ ] Reinstall service
- [ ] Restart máy

---

## 📞 BÁO CÁO LỖI CHO DEV

Khi không tự xử lý được, gửi cho DEV:

1. **File log đầy đủ:**
   ```powershell
   Copy-Item "C:\ProgramData\DongleSyncService\logs\service-*.log" -Destination "E:\Logs_Backup\"
   ```

2. **System info:**
   ```powershell
   Get-ComputerInfo | Select-Object WindowsVersion, OsArchitecture
   Get-Service DongleSyncService | Format-List *
   Get-Item "C:\Users\$env:USERNAME\AppData\Roaming\CHCNAV\CHC Geomatics Office 2\CHC.CGO.Common.dll" | Select-Object *
   ```

3. **Screenshot:**
   - Services Manager (status của service)
   - Event Viewer (lỗi Application)
   - Thông báo lỗi từ CHC (nếu có)

4. **USB info:**
   ```powershell
   Get-Volume | Where-Object DriveType -eq 'Removable'
   Get-ChildItem "D:\dongle\" | Select-Object Name, Length  # Thay D: cho đúng
   ```

---

**Ghi chú:** Hầu hết lỗi đều do Antivirus, ReadOnly, hoặc USB không đúng cấu trúc. Check 3 điều này trước!

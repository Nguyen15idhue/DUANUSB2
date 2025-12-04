# USB Dongle Security System - Tổng Quan Kỹ Thuật

## 📋 Mô Tả Hệ Thống

Hệ thống bảo mật USB Dongle cho phép bảo vệ phần mềm bằng cách yêu cầu USB dongle vật lý phải được cắm vào máy tính để ứng dụng có thể hoạt động. Hệ thống tự động đồng bộ file DLL được mã hóa từ USB dongle vào máy tính khi phát hiện dongle hợp lệ.

## 🏗️ Kiến Trúc Hệ Thống

### Thành Phần Chính

1. **DongleSyncService** (Windows Service)
   - Chạy nền tự động khi khởi động Windows
   - Giám sát USB dongle liên tục
   - Xác thực và đồng bộ DLL

2. **DongleCreatorTool** (Desktop Application)
   - Công cụ tạo USB dongle cho administrator
   - Mã hóa và ghi file vào USB
   - Quản lý khóa bảo mật

3. **USB Dongle** (USB Flash Drive)
   - Lưu trữ DLL được mã hóa
   - Chứa khóa xác thực phần cứng
   - Thông tin cấu hình và metadata

## 🔒 5 Lớp Bảo Mật

### Layer 1: USB Hardware ID Validation
**Chức năng:** Xác thực định danh phần cứng của USB dongle

**Cơ chế:**
- Sử dụng PNPDeviceID từ Windows Management Instrumentation (WMI)
- PNPDeviceID là định danh phần cứng ổn định, không thay đổi khi rút/cắm lại USB
- Tạo khóa SHA-256 từ PNPDeviceID để nhận dạng duy nhất từng USB

**Quy trình:**
```
USB cắm vào → Đọc PNPDeviceID từ WMI → Hash SHA-256 
→ So sánh với dongle.key → Chấp nhận/Từ chối
```

**Bảo vệ khỏi:** Sao chép file sang USB khác, giả mạo dongle

---

### Layer 2: AES-256 Encryption
**Chức năng:** Mã hóa file DLL trên USB dongle

**Cơ chế:**
- Thuật toán: AES-256-CBC (Advanced Encryption Standard)
- Key size: 256-bit (32 bytes)
- IV (Initialization Vector): 128-bit (16 bytes) random

**Quy trình mã hóa:**
```
DLL gốc → AES-256-CBC Encrypt (Key + IV) 
→ patch.dll.enc (file mã hóa trên USB)
```

**Quy trình giải mã:**
```
patch.dll.enc từ USB → AES-256-CBC Decrypt (Key + IV) 
→ DLL gốc → Lưu vào ổ C:
```

**Bảo vệ khỏi:** Đọc trực tiếp DLL từ USB, phân tích ngược kỹ thuật

---

### Layer 3: Machine Binding
**Chức năng:** Ràng buộc dongle với một máy tính cụ thể

**Cơ chế:**
- Lần đầu cắm USB vào máy: Tạo "vân tay" máy tính từ:
  - CPU ID (Processor ID)
  - BIOS Serial Number
  - Motherboard Serial Number
- Lưu "vân tay" vào file `bind.key` trên máy tính
- Lần sau: So sánh hardware fingerprint hiện tại với bind.key

**Quy trình:**
```
Lần đầu: Thu thập CPU ID + BIOS + Motherboard 
→ Hash SHA-256 → Lưu bind.key

Lần sau: Thu thập hardware info → Hash → So sánh với bind.key
→ Khớp: OK | Không khớp: Từ chối
```

**Bảo vệ khỏi:** Chuyển USB sang máy tính khác, sử dụng đồng thời nhiều máy

**Đặc biệt:** Admin có thể xóa bind.key để đổi sang máy khác (qua DongleCreatorTool)

---

### Layer 4: Runtime Heartbeat Monitor
**Chức năng:** Giám sát USB trong quá trình ứng dụng đang chạy

**Cơ chế:**
- Kiểm tra sự hiện diện của USB mỗi **3 giây** (có thể cấu hình)
- Gửi tín hiệu "heartbeat" để xác nhận USB vẫn còn cắm
- Nếu USB bị rút: Ngay lập tức restore DLL gốc (xóa DLL đã patch)

**Quy trình:**
```
Ứng dụng đang chạy → Service kiểm tra USB (mỗi 3s)
→ USB còn: Tiếp tục | USB mất: Restore DLL gốc ngay lập tức
```

**Bảo vệ khỏi:** Sao chép DLL sau khi đồng bộ, sử dụng offline

---

### Layer 5: DLL Integrity Check (Anti-Tampering)
**Chức năng:** Phát hiện và ngăn chặn việc thay thế DLL đã patch bằng cách thủ công

**Cơ chế:**
- **SHA-256 Hash Verification:** Tính hash của DLL mỗi khi heartbeat (3 giây/lần)
- **Timestamp Verification:** So sánh thời gian sửa đổi file với thời gian patch ban đầu
- **Auto-Restore on Tampering:** Tự động phục hồi DLL gốc nếu phát hiện sửa đổi
- **Grace Period:** Cho phép 5 giây chênh lệch timestamp để tránh false positive

**Quy trình:**
```
Heartbeat → Tính SHA-256(DLL hiện tại) → So sánh với hash đã lưu
→ Khớp: OK | Không khớp: ⚠️ TAMPERING DETECTED!
→ Tự động restore DLL gốc + Tắt ứng dụng
```

**Kịch bản tấn công bị ngăn chặn:**
```
1. User cắm USB → DLL được patch (hash: ABC123, timestamp: T0)
2. User copy DLL đã patch ra desktop
3. User rút USB → Service restore DLL gốc
4. User paste DLL từ desktop → Thay thế DLL gốc
5. [3 giây sau] Heartbeat phát hiện:
   - Hash hiện tại ≠ ABC123 (hoặc)
   - Timestamp hiện tại > T0 + 5s
   → ⚠️ Integrity violation → Auto-restore + Kill app
```

**Bảo vệ khỏi:** 
- Copy/paste DLL để bypass USB requirement
- Manual file replacement attacks
- Offline usage after obtaining patched DLL

**Cơ chế Auto-Close App:**
- Khi USB được cắm vào, service tự động kiểm tra DLL có đang được sử dụng không
- Nếu app đang chạy (DLL locked): Service tự động đóng app (3 lần thử)
  - Lần 1-2: Đóng graceful (CloseMainWindow)
  - Lần 3: Force kill (Process.Kill)
- Sau khi đóng app thành công, service patch DLL và hiển thị notification
- Người dùng mở lại app để sử dụng tính năng mới

---

## 📁 Cấu Trúc File

### Trên USB Dongle
```
D:\
├── config.json          # Metadata: GUID, version, timestamp
├── dongle.key          # USB Hardware Key (SHA-256 của PNPDeviceID)
├── patch.dll.enc       # DLL được mã hóa AES-256
└── iv.bin              # Initialization Vector cho AES
```

### Trên Máy Tính
```
C:\ProgramData\DongleSyncService\
├── bind.key                    # Machine binding fingerprint
├── synced_patch.dll            # DLL đã giải mã (xóa khi rút USB)
├── logs\                       # Log files (1 file/ngày)
│   └── log-20251204.txt
└── backups\                    # DLL backups (tối đa 5 bản)
    └── synced_patch_backup_*.dll

C:\Program Files\CHC Geomatics\Dongle Service\
├── DongleSyncService.exe       # Windows Service
├── DongleSyncService.dll
└── [dependencies...]
```

## 🔄 Quy Trình Hoạt Động

### 1. Tạo USB Dongle (Administrator)
```
1. Mở DongleCreatorTool.exe
2. Chọn USB drive (ví dụ: D:\)
3. Chọn file DLL cần bảo vệ
4. Click "Create Dongle"
   → Tạo GUID mới
   → Đọc PNPDeviceID của USB
   → Hash SHA-256 → Tạo dongle.key
   → Mã hóa DLL bằng AES-256 → patch.dll.enc
   → Ghi config.json, iv.bin
   → Tự động xóa bind.key cũ (nếu có)
5. USB dongle sẵn sàng sử dụng
```

### 2. Sử dụng USB Dongle (End User)

#### Lần Đầu Tiên (Binding)
```
1. Cài đặt DongleSyncService-Setup.msi
   → Service tự động khởi động
2. Cắm USB dongle vào máy tính
3. Service phát hiện USB:
   → Kiểm tra file config.json, dongle.key
   → Đọc PNPDeviceID và hash
   → So sánh với dongle.key → OK
   → Kiểm tra bind.key: KHÔNG TỒN TẠI
   → Thu thập hardware info (CPU, BIOS, Motherboard)
   → Tạo bind.key và lưu
   → Kiểm tra app có đang chạy không
   → Nếu app đang chạy: Tự động đóng app (với notification)
   → Giải mã patch.dll.enc
   → Backup DLL gốc
   → Patch DLL mới vào thư mục app
   → Hiển thị notification: "Features updated successfully"
4. Người dùng mở lại app và sử dụng tính năng mới
```

#### Lần Sau (Validation)
```
1. Cắm USB dongle
2. Service kiểm tra:
   → Hardware ID: OK (PNPDeviceID khớp)
   → Machine Binding: OK (Hardware fingerprint khớp bind.key)
   → Kiểm tra app có đang chạy
   → Auto-close app nếu cần (3 attempts)
   → Giải mã và patch DLL
   → Hiển thị notification thành công
3. Runtime monitoring bắt đầu (kiểm tra mỗi 3s)
4. Nếu rút USB:
   → Service phát hiện ngay lập tức (trong 3s)
   → Restore DLL gốc từ backup
   → Hiển thị notification: "USB Dongle Removed"
   → App sẽ hoạt động với tính năng cơ bản (DLL gốc)
```

### 3. Chuyển Sang Máy Khác (Admin)
```
1. Chạy DongleCreatorTool.exe trên máy CŨ
2. Click "Clear Machine Binding"
   → Xóa C:\ProgramData\DongleSyncService\bind.key
3. Rút USB từ máy cũ
4. Cắm USB vào máy MỚI
5. Service trên máy mới tự động binding lại
```

## 🛡️ Ma Trận Bảo Mật

| Kịch Bản Tấn Công | Layer Chống | Kết Quả |
|-------------------|-------------|---------|
| Sao chép file từ USB sang USB khác | Layer 1 (Hardware ID) | ❌ Thất bại: dongle.key không khớp |
| Đọc trực tiếp patch.dll.enc | Layer 2 (AES-256) | ❌ Thất bại: File bị mã hóa |
| Chuyển USB sang máy khác | Layer 3 (Machine Binding) | ❌ Thất bại: bind.key không khớp |
| Sao chép DLL sau khi giải mã | Layer 4 (Heartbeat) | ❌ Thất bại: DLL tự xóa khi rút USB |
| Crack DLL trên RAM | Layer 2 + Layer 4 | ⚠️ Khó: Cần reverse engineering + USB phải cắm |
| Giả mạo WMI/Hardware Info | Multi-layer | ⚠️ Rất khó: Cần quyền admin + kernel-level hook |

## ⚙️ Thông Số Kỹ Thuật

### Hệ Thống Yêu Cầu
- **OS:** Windows 10/11 (64-bit)
- **Framework:** .NET 8.0
- **Quyền:** Administrator (chỉ khi cài đặt)
- **USB:** Bất kỳ USB flash drive nào (tối thiểu 16MB trống)

### Hiệu Năng
- **Thời gian đồng bộ DLL:** < 2 giây (file 10MB)
- **RAM sử dụng:** ~100-110MB (service với auto-close)
- **CPU usage:** < 1% (idle), < 5% (sync operation)
- **Heartbeat interval:** 3 giây (configurable)
- **USB detection delay:** < 1 giây
- **Auto-close retry:** 3 attempts (2 graceful + 1 force)
- **Retry delay:** 2 seconds between attempts

### Mã Hóa
- **Algorithm:** AES-256-CBC
- **Key derivation:** SHA-256
- **Random IV:** 16 bytes (mỗi dongle khác nhau)
- **Encryption library:** .NET System.Security.Cryptography

### Logging
- **Format:** Serilog text format
- **Rotation:** Daily (1 file/ngày)
- **Retention:** Không giới hạn (người dùng tự quản lý)
- **Path:** `C:\ProgramData\DongleSyncService\logs\`

## 🔧 Cấu Hình Nâng Cao

### Thay Đổi Heartbeat Interval
Mặc định: 3 giây. Để thay đổi:
1. Sửa file source code: `src\DongleSyncService\Utils\Constants.cs`
2. Thay đổi: `public const int HeartbeatInterval = 3000;` (đơn vị: milliseconds)
3. Rebuild service: `dotnet publish -c Release`
4. Rebuild MSI và reinstall service

### Quản Lý Backups
- Tự động: Giữ 5 bản backup mới nhất
- Thủ công: Xóa file trong `C:\ProgramData\DongleSyncService\backups\`

### Kiểm Tra Log
```powershell
# Xem log hôm nay
Get-Content "C:\ProgramData\DongleSyncService\logs\log-$(Get-Date -Format 'yyyyMMdd').txt" -Tail 50

# Tìm lỗi
Select-String -Path "C:\ProgramData\DongleSyncService\logs\*.txt" -Pattern "ERROR|FATAL"
```

## 🐛 Troubleshooting

### Service Không Khởi Động
**Kiểm tra:**
```powershell
Get-Service DongleSyncService
Get-EventLog -LogName Application -Source DongleSyncService -Newest 10
```

**Nguyên nhân thường gặp:**
- Thiếu .NET 8.0 Runtime
- Quyền truy cập bị chặn bởi antivirus
- Cổng IPC bị chiếm dụng

### USB Không Được Phát Hiện
**Kiểm tra:**
1. USB có chứa đầy đủ file: config.json, dongle.key, patch.dll.enc, iv.bin
2. Chạy WMI query kiểm tra PNPDeviceID:
```powershell
Get-WmiObject Win32_DiskDrive | Where-Object {$_.InterfaceType -eq "USB"} | Select-Object PNPDeviceID
```

### Machine Binding Lỗi
**Nguyên nhân:** Hardware thay đổi (CPU, BIOS, Mainboard)

**Giải pháp:**
```powershell
# Xóa bind.key để binding lại
Remove-Item "C:\ProgramData\DongleSyncService\bind.key" -Force
# Hoặc dùng DongleCreatorTool → Clear Machine Binding
```

## 📊 Giám Sát Hệ Thống

### Kiểm Tra Trạng Thái
```powershell
# Service status
Get-Service DongleSyncService | Format-List *

# Check if DLL is synced
Test-Path "C:\ProgramData\DongleSyncService\synced_patch.dll"

# Check binding
Test-Path "C:\ProgramData\DongleSyncService\bind.key"
```

### Metrics Quan Trọng
- **Service Uptime:** Nên 99.9%+ (restart cùng Windows)
- **Sync Success Rate:** Nên 100% (log không có ERROR)
- **Heartbeat Failures:** Nên 0 (USB không bị ngắt kết nối)

## 🔐 Best Practices Bảo Mật

### Cho Administrator
1. **Backup khóa mã hóa:** Lưu AES key và IV ở nơi an toàn (ngoài USB)
2. **Giới hạn số dongle:** Tạo đúng số lượng license cần thiết
3. **Log monitoring:** Định kỳ kiểm tra log phát hiện bất thường
4. **Version control:** Ghi rõ version trong config.json khi update DLL

### Cho End User
1. **Không chia sẻ USB:** Mỗi máy cần USB riêng (machine binding)
2. **Không rút USB khi đang dùng:** Ứng dụng sẽ crash ngay lập tức
3. **Backup quan trọng:** Service có thể xóa DLL bất kỳ lúc nào
4. **Báo lỗi sớm:** Liên hệ admin ngay khi có lỗi xác thực

## 📈 Khả Năng Mở Rộng

### Đã Triển Khai (v1.0.1)
- ✅ Auto-close app mechanism (graceful + force kill)
- ✅ DLL patch with retry logic (3 attempts)
- ✅ Windows toast notifications (PowerShell-based)
- ✅ Configurable heartbeat interval (3 seconds default)
- ✅ Auto-restore DLL on USB removal
- ✅ **DLL Integrity Check** - Anti-tampering protection (SHA-256 + timestamp verification)

### Có Thể Thêm
- ⏳ Cloud license validation (online check)
- ⏳ Expiration date cho dongle (license hết hạn)
- ⏳ Multiple DLL support (đồng bộ nhiều file)
- ⏳ Remote revocation (admin thu hồi license từ xa)
- ⏳ Audit trail (ghi lại lịch sử sử dụng chi tiết)

### Hạn Chế Hiện Tại
- ❌ Chỉ Windows (không hỗ trợ macOS/Linux)
- ❌ Cần USB cắm liên tục (không offline mode)
- ❌ Một máy chỉ bind với một USB (không multi-dongle)
- ❌ Machine binding cứng (đổi hardware phải rebind thủ công)

## 📞 Hỗ Trợ Kỹ Thuật

**Developer:** CHC Geomatics Development Team  
**Version:** 1.0.1  
**Release Date:** December 4, 2025  
**License:** Proprietary

---

*Tài liệu kỹ thuật này mô tả kiến trúc và cơ chế hoạt động của hệ thống bảo mật USB Dongle. Để biết hướng dẫn sử dụng chi tiết, vui lòng tham khảo USER-GUIDE.md*

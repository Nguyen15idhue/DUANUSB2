# Hướng Dẫn Sử Dụng Hệ Thống USB Dongle

## 📋 Tổng Quan

Tài liệu này gồm 2 phần:
- **PHẦN A:** Tạo USB Dongle (dành cho DEV/ADMIN)
- **PHẦN B:** Cài đặt và sử dụng (dành cho USER)

---

# PHẦN A: TẠO USB DONGLE (Dành cho DEV)

## 🔧 A1. Chuẩn Bị

**Yêu cầu:**
- USB trống (khuyến nghị ≥ 4GB)
- File DLL gốc: `CHC.CGO.Common.dll` (287 KB)
- Tool: `DongleCreatorTool.exe`

## 🎯 A2. Tạo Dongle

1. **Chạy DongleCreatorTool.exe** với quyền Admin

2. **Tab "Create Dongle":**
   - **DLL File:** Browse → chọn `CHC.CGO.Common.dll` gốc
   - **USB Drive:** Chọn ổ USB từ dropdown
   - **Machine Fingerprint:** (Để trống nếu không bind máy cụ thể)
   - Click **"Create Dongle"**

3. **Chờ 5-10 giây** → Thông báo "Dongle created successfully!"

4. **Kiểm tra USB:**
   ```
   USB:\dongle\
   ├── patch.dll.enc    (286 KB - DLL đã mã hóa)
   ├── iv.bin           (16 bytes - IV)
   └── dongle.key       (32 bytes - Key)
   ```

## ✅ A3. Kiểm Tra Dongle

**Cắm USB vào máy khác và verify:**

1. Chạy `DongleCreatorTool.exe`
2. Tab **"Verify Dongle"**
3. Chọn USB drive → Click **"Verify"**
4. ✅ Thấy: "Dongle is valid!"

---

# PHẦN B: CÀI ĐẶT & SỬ DỤNG (Dành cho USER)

## 📦 B1. Cài Đặt Service

**Yêu cầu:**
- Windows 10/11
- Quyền Administrator
- CHC Geomatics Office 2 đã cài đặt

**Các bước:**

1. Click chuột phải `DongleSyncService-Setup-v1.0.0.exe` → **Run as administrator**
2. Làm theo wizard: Next → Accept → Next → Install → Finish
3. Service tự động chạy

## ✅ B2. Kiểm Tra Service Đang Chạy

1. Nhấn `Win+R` → gõ `services.msc` → Enter
2. Tìm **"USB Dongle Sync Service"**
3. Status phải là **"Running"**

## 🔧 B2 (Backup): Cài Đặt Phần Mềm

### 2.1. Gỡ Cài Đặt Phiên Bản Cũ (Nếu Có)

**Nếu bạn đã cài đặt phiên bản cũ trước đó:**

1. Nhấn `Windows + R`
2. Gõ: `appwiz.cpl` và nhấn Enter
3. Tìm **"USB Dongle Sync Service"**
4. Click chuột phải → Chọn **"Uninstall"**
5. Làm theo hướng dẫn để gỡ cài đặt

### 2.2. Cài Đặt Phiên Bản Mới

1. **Click chuột phải** vào file `DongleSyncService-Setup-v1.0.0.exe`
2. Chọn **"Run as administrator"** (Chạy với quyền quản trị viên)

   ![Run as Admin](https://via.placeholder.com/400x100/4CAF50/FFFFFF?text=Click+Chuột+Phải+→+Run+as+Administrator)

3. Nếu Windows hỏi **"Do you want to allow this app to make changes?"**
   → Nhấn **"Yes"**

4. **Cửa sổ cài đặt xuất hiện:**

   📌 **Trang 1 - Welcome:**
   - Đọc thông tin
   - Nhấn **"Next"**

   📌 **Trang 2 - License Agreement:**
   - Chọn **"I accept the agreement"**
   - Nhấn **"Next"**

   📌 **Trang 3 - Destination:**
   - Để mặc định: `C:\Program Files\CHC Geomatics\Dongle Service\`
   - Nhấn **"Next"**

   📌 **Trang 4 - Ready to Install:**
   - Xem lại thông tin
   - Nhấn **"Install"**

   ⏳ **Đợi 10-15 giây** để cài đặt

   📌 **Trang 5 - Completing:**
   - Có thể tick **"View log files"** nếu muốn xem log ngay
   - Nhấn **"Finish"**

5. **Cài đặt hoàn tất!** ✅

---

## ✅ Bước 3: Kiểm Tra Dịch Vụ Đã Chạy

### 3.1. Mở Services Manager

1. Nhấn `Windows + R`
2. Gõ: `services.msc` và nhấn Enter
3. Tìm dịch vụ: **"USB Dongle Sync Service"**

### 3.2. Kiểm Tra Trạng Thái

Dịch vụ phải có thông tin như sau:

| Thuộc Tính | Giá Trị Mong Đợi |
|------------|------------------|
| **Status** | Running (Đang chạy) |
| **Startup Type** | Automatic (Tự động) |
| **Log On As** | Local System |

**Nếu dịch vụ KHÔNG chạy:**
- Click chuột phải vào dịch vụ
- Chọn **"Start"**
- Đợi 2-3 giây

---

## 🧪 Bước 4: Kiểm Tra Chức Năng

### 4.1. Chuẩn Bị USB Dongle

📍 **Lưu ý quan trọng:**
- USB dongle phải có thư mục `dongle\` ở thư mục gốc
- Bên trong phải có 3 file:
  - `patch.dll.enc` (file DLL đã mã hóa)
  - `iv.bin` (initialization vector)
  - `dongle.key` (khóa giải mã)

**Kiểm tra USB dongle:**
1. Cắm USB vào máy tính
2. Mở File Explorer
3. Vào ổ USB (ví dụ: `D:\`)
4. Kiểm tra có thư mục `dongle\` với 3 file trên

### 4.2. Kịch Bản Kiểm Tra 1 - Cắm USB Dongle

**Mục đích:** Kiểm tra dịch vụ tự động patch DLL khi phát hiện USB

**Các bước thực hiện:**

1. **Đảm bảo dịch vụ đang chạy** (xem Bước 3)

2. **Cắm USB Dongle vào máy tính**
   - Đợi Windows nhận dạng USB (5-10 giây)
   - Đèn LED trên USB sẽ sáng

3. **Chờ 2-3 giây** để dịch vụ xử lý

4. **Kiểm tra file log:**
   - Nhấn `Windows + R`
   - Gõ: `notepad C:\ProgramData\DongleSyncService\logs\service-20251204.log`
   - (Thay `20251204` bằng ngày hôm nay theo định dạng YYYYMMDD)

5. **Tìm các dòng log quan trọng:**

   ✅ **Khi thành công, bạn sẽ thấy:**
   ```
   [INF] USB with Dongle folder detected: D:\dongle
   [INF] Validating USB dongle files...
   [INF] All dongle files present and valid
   [DBG] Removing ReadOnly attribute from DLL
   [INF] DLL patched successfully
   [INF] Patch applied: CHC.CGO.Common.dll
   ```

   ❌ **Nếu có lỗi, có thể thấy:**
   ```
   [ERR] Failed to patch DLL
   [ERR] Access to the path '...' is denied
   [WRN] Dongle files not found
   ```

6. **Kiểm tra file DLL đã được patch:**
   - Mở File Explorer
   - Dán đường dẫn: `C:\Users\ADMIN\AppData\Roaming\CHCNAV\CHC Geomatics Office 2\`
   - Tìm file: `CHC.CGO.Common.dll`
   - Click chuột phải → Properties
   - Kiểm tra:
     - **Size:** Phải là **286 KB** (293,888 bytes) - đây là file đã patch
     - **Date modified:** Phải vừa thay đổi (trong vài phút gần đây)

7. **Kiểm tra file backup:**
   - Mở File Explorer
   - Dán đường dẫn: `C:\ProgramData\DongleSyncService\backups\`
   - Phải có file: `CHC.CGO.Common.dll.original`
   - Và file metadata: `CHC.CGO.Common.dll.original.meta`

### 4.3. Kịch Bản Kiểm Tra 2 - Mở Phần Mềm CHC

**Mục đích:** Kiểm tra phần mềm hoạt động bình thường với DLL đã patch

**Các bước thực hiện:**

1. **Đảm bảo USB Dongle đã cắm và DLL đã patch** (xem Bước 4.2)

2. **Mở phần mềm CHC Geomatics Office 2**
   - Start Menu → CHC Geomatics Office 2
   - Hoặc click icon trên Desktop

3. **Kiểm tra phần mềm:**
   - Phần mềm khởi động **bình thường**
   - Không có thông báo lỗi license
   - Tất cả chức năng hoạt động

4. **Thử các chức năng cơ bản:**
   - Tạo project mới
   - Mở file dữ liệu
   - Sử dụng các công cụ xử lý
   - Xuất dữ liệu

   ✅ **Tất cả phải hoạt động bình thường**

### 4.4. Kịch Bản Kiểm Tra 3 - Rút USB Dongle

**Mục đích:** Kiểm tra dịch vụ tự động restore DLL gốc khi rút USB

**Các bước thực hiện:**

1. **Đóng phần mềm CHC Geomatics Office 2** (nếu đang mở)
   - Đảm bảo thoát hoàn toàn, không còn chạy nền

2. **Rút USB Dongle ra khỏi máy tính**
   - Safely Remove Hardware (nếu muốn)
   - Hoặc rút trực tiếp

3. **Chờ 2-3 giây**

4. **Kiểm tra file log:**
   - Mở lại file log như Bước 4.2
   - Cuộn xuống cuối file

5. **Tìm các dòng log:**

   ✅ **Khi thành công:**
   ```
   [INF] USB removed: D:\
   [DBG] Removing ReadOnly attribute from destination DLL
   [INF] DLL restored successfully from backup
   [INF] Restored: CHC.CGO.Common.dll
   ```

6. **Kiểm tra file DLL đã được restore:**
   - Mở File Explorer
   - Vào: `C:\Users\ADMIN\AppData\Roaming\CHCNAV\CHC Geomatics Office 2\`
   - File: `CHC.CGO.Common.dll`
   - Kiểm tra:
     - **Size:** Phải là **287 KB** (294,400 bytes) - đây là file gốc
     - **Date modified:** Vừa thay đổi

7. **Thử mở lại phần mềm CHC (không có USB):**
   - Mở CHC Geomatics Office 2
   - **Sẽ thấy thông báo lỗi license** (đây là hành vi đúng)
   - Đóng phần mềm

### 4.5. Kịch Bản Kiểm Tra 4 - Cắm Lại USB

**Mục đích:** Kiểm tra chu trình hoạt động liên tục

**Các bước thực hiện:**

1. **Cắm lại USB Dongle**
2. **Chờ 2-3 giây**
3. **Kiểm tra log** - phải thấy "DLL patched successfully"
4. **Mở lại CHC Geomatics Office 2** - phải hoạt động bình thường
5. **Lặp lại 2-3 lần** để chắc chắn

---

## 📊 Bước 5: Kiểm Tra Heartbeat Monitor

**Mục đích:** Đảm bảo dịch vụ tự động phát hiện khi USB bị rút trong khi phần mềm đang chạy

### 5.1. Kịch Bản Kiểm Tra Heartbeat

1. **Cắm USB Dongle**
2. **Mở phần mềm CHC Geomatics Office 2**
3. **Để phần mềm chạy** (không đóng)
4. **Rút USB Dongle** trong khi phần mềm đang mở
5. **Chờ 2-5 giây**
6. **Kiểm tra log:**

   ✅ **Phải thấy:**
   ```
   [WRN] Heartbeat check failed - USB may have been removed
   [INF] USB removed: D:\
   [INF] DLL restored successfully from backup
   ```

7. **Phần mềm CHC sẽ:**
   - Có thể hiển thị lỗi license
   - Hoặc tiếp tục chạy nhưng một số chức năng bị khóa
   - **Đây là hành vi đúng**

---

## 🛠️ Bước 6: Xử Lý Sự Cố

### Vấn Đề 1: Dịch Vụ Không Khởi Động

**Triệu chứng:** Status = "Stopped" trong Services Manager

**Cách khắc phục:**

1. Mở Command Prompt với quyền Admin:
   - Nhấn `Windows + X`
   - Chọn **"Windows PowerShell (Admin)"**

2. Chạy lệnh:
   ```powershell
   sc start DongleSyncService
   ```

3. Xem log lỗi:
   ```powershell
   notepad C:\ProgramData\DongleSyncService\logs\service-20251204.log
   ```
   (Thay ngày cho đúng)

4. Chụp màn hình log → Gửi cho người phát triển

### Vấn Đề 2: USB Được Cắm Nhưng DLL Không Patch

**Triệu chứng:** Log không có "DLL patched successfully"

**Cách kiểm tra:**

1. **Kiểm tra USB dongle:**
   - Đảm bảo có thư mục `dongle\`
   - Có đủ 3 file: `patch.dll.enc`, `iv.bin`, `dongle.key`

2. **Kiểm tra quyền file:**
   - Vào thư mục: `C:\Users\ADMIN\AppData\Roaming\CHCNAV\CHC Geomatics Office 2\`
   - Click chuột phải file `CHC.CGO.Common.dll`
   - Properties → Tab **General**
   - Bỏ tick **"Read-only"** nếu có
   - Nhấn **Apply**

3. **Rút USB và cắm lại**

4. **Xem log** để kiểm tra

### Vấn Đề 3: Access Denied / Permission Error

**Triệu chứng:** Log có "Access to the path is denied"

**Cách khắc phục:**

1. **Kiểm tra file Read-only:**
   - Làm theo Vấn Đề 2 → Bước 2

2. **Restart dịch vụ:**
   - Mở Services Manager (`services.msc`)
   - Click chuột phải **"USB Dongle Sync Service"**
   - Chọn **"Restart"**

3. **Cắm lại USB**

### Vấn Đề 4: Phần Mềm CHC Không Chạy Sau Khi Patch

**Triệu chứng:** CHC Geomatics Office 2 báo lỗi hoặc crash

**Cách khắc phục:**

1. **Rút USB Dongle** → DLL sẽ được restore về gốc

2. **Kiểm tra backup:**
   ```
   C:\ProgramData\DongleSyncService\backups\CHC.CGO.Common.dll.original
   ```
   - File này phải có kích thước **287 KB**

3. **Thử cắm lại USB**

4. **Nếu vẫn lỗi:**
   - Chụp màn hình lỗi
   - Chụp log file
   - Gửi cho người phát triển

---

## 📝 Bước 7: Báo Cáo Kết Quả Test

### 7.1. Thông Tin Cần Ghi Lại

Sau khi test, hãy ghi lại các thông tin sau:

| Mục | Kết Quả | Ghi Chú |
|-----|---------|---------|
| **Cài đặt thành công** | ✅ / ❌ | |
| **Dịch vụ tự động chạy** | ✅ / ❌ | |
| **Cắm USB → Patch DLL** | ✅ / ❌ | Thời gian: ___ giây |
| **CHC mở được (có USB)** | ✅ / ❌ | |
| **Rút USB → Restore DLL** | ✅ / ❌ | Thời gian: ___ giây |
| **CHC báo lỗi (không USB)** | ✅ / ❌ | |
| **Heartbeat monitor hoạt động** | ✅ / ❌ | |
| **Cắm lại USB nhiều lần** | ✅ / ❌ | Số lần test: ___ |

### 7.2. File Cần Gửi Cho Developer

Khi báo cáo lỗi hoặc hoàn thành test, hãy gửi:

1. **File log:**
   ```
   C:\ProgramData\DongleSyncService\logs\service-YYYYMMDD.log
   ```
   (File của ngày test)

2. **Ảnh chụp màn hình:**
   - Services Manager (trạng thái dịch vụ)
   - Event Viewer (nếu có lỗi)
   - Thông báo lỗi từ CHC Geomatics Office 2 (nếu có)

3. **Thông tin hệ thống:**
   - Phiên bản Windows: (Nhấn `Windows + R` → gõ `winver`)
   - Phiên bản CHC Geomatics Office 2
   - Kích thước file DLL trước và sau patch

---

## 📚 Phụ Lục

### A. Các File và Thư Mục Quan Trọng

| Đường Dẫn | Mô Tả |
|-----------|-------|
| `C:\Program Files\CHC Geomatics\Dongle Service\` | Thư mục cài đặt dịch vụ |
| `C:\ProgramData\DongleSyncService\logs\` | Thư mục chứa file log |
| `C:\ProgramData\DongleSyncService\backups\` | Thư mục chứa backup DLL gốc |
| `C:\Users\ADMIN\AppData\Roaming\CHCNAV\CHC Geomatics Office 2\` | Thư mục chứa DLL cần patch |
| `D:\dongle\` | Thư mục dongle trên USB (ví dụ) |

### B. Các Lệnh Hữu Ích

**Xem trạng thái dịch vụ:**
```powershell
Get-Service DongleSyncService
```

**Khởi động dịch vụ:**
```powershell
Start-Service DongleSyncService
```

**Dừng dịch vụ:**
```powershell
Stop-Service DongleSyncService
```

**Xem log realtime:**
```powershell
Get-Content C:\ProgramData\DongleSyncService\logs\service-20251204.log -Wait -Tail 20
```
(Nhấn `Ctrl+C` để thoát)

**Kiểm tra file DLL:**
```powershell
Get-Item "C:\Users\ADMIN\AppData\Roaming\CHCNAV\CHC Geomatics Office 2\CHC.CGO.Common.dll" | Select-Object Length, LastWriteTime
```

### C. Các Kích Thước File Quan Trọng

| File | Kích Thước | Ghi Chú |
|------|-----------|---------|
| **CHC.CGO.Common.dll** (Gốc) | 287 KB (294,400 bytes) | File chưa patch |
| **CHC.CGO.Common.dll** (Patched) | 286 KB (293,888 bytes) | File đã patch |
| **patch.dll.enc** | ~286 KB | File trên USB dongle |

---

## 💡 Lời Khuyên

1. **Test nhiều lần:** Cắm - rút USB ít nhất 5 lần để đảm bảo ổn định
2. **Đọc log:** Luôn kiểm tra log sau mỗi thao tác
3. **Backup quan trọng:** Không xóa thư mục `backups\`
4. **Chụp ảnh:** Chụp màn hình mọi lỗi gặp phải
5. **Báo cáo sớm:** Gặp lỗi lạ → báo ngay cho developer

---

## 📞 Liên Hệ Hỗ Trợ

Nếu gặp vấn đề không giải quyết được, hãy liên hệ:

- **Gửi log file** và **ảnh chụp màn hình lỗi**
- **Mô tả chi tiết** các bước đã làm
- **Ghi rõ** môi trường test (Windows version, CHC version)

---

**Chúc bạn test thành công! 🎉**

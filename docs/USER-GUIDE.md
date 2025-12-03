# 📘 HƯỚNG DẪN SỬ DỤNG - USB DONGLE CHO CHC GEOMATICS OFFICE 2

## 🎯 TỔNG QUAN

Hệ thống USB Dongle cho phép mở rộng tính năng của **CHC Geomatics Office 2** thông qua USB được cấp phép.

### Người dùng nhận được gì?

**1. File cài đặt (MSI Installer)**
```
DongleSystem-Installer.msi   (khoảng 5-10 MB)
```

**2. USB Dongle được cấp phép**
```
USB:\dongle\
  ├── config.json          (Cấu hình dongle)
  ├── dongle.key           (Khóa phần cứng USB)
  ├── patch.dll.enc        (File DLL mã hóa)
  ├── iv.bin               (Khóa giải mã)
  └── README.txt           (Hướng dẫn)
```

**3. Tài liệu hướng dẫn**
- `USER-GUIDE.md` (file này)
- `TROUBLESHOOTING.md` (xử lý sự cố)

---

## 📋 YÊU CẦU HỆ THỐNG

- **Hệ điều hành:** Windows 10/11 (64-bit)
- **Phần mềm:** CHC Geomatics Office 2 đã cài đặt
- **USB:** USB dongle được cấp phép chính thức
- **Quyền:** Administrator để cài đặt service

---

## 🚀 CÀI ĐẶT

### Bước 1: Cài đặt DongleSystem

1. **Chạy installer**
   - Nhấp đúp vào `DongleSystem-Installer.msi`
   - Nhấn `Next` → `Install` → Nhập mật khẩu Administrator nếu được yêu cầu

2. **Xác nhận cài đặt thành công**
   - Service `USB Dongle Sync Service` đã được cài đặt
   - Kiểm tra: Mở `services.msc` → tìm "USB Dongle Sync Service" → Status = "Running"

### Bước 2: Kiểm tra CHC Geomatics Office 2

1. Mở **CHC Geomatics Office 2**
2. Xác nhận phần mềm chạy bình thường
3. Đóng phần mềm

### Bước 3: Kích hoạt với USB Dongle

1. **Cắm USB dongle** vào cổng USB
2. Đợi 5-10 giây để hệ thống nhận diện
3. Kiểm tra thông báo:
   - Windows sẽ hiển thị "USB device connected"
   - Service sẽ tự động kích hoạt tính năng mở rộng

4. **Mở CHC Geomatics Office 2**
   - Các tính năng mở rộng đã được kích hoạt
   - Kiểm tra menu/toolbar để thấy các chức năng mới

---

## ✅ SỬ DỤNG HÀNG NGÀY

### Quy trình làm việc chuẩn

**Bắt đầu ngày làm việc:**
```
1. Cắm USB dongle vào máy
2. Đợi 5-10 giây
   - Mở **CHC Geomatics Office 2**
4. Sử dụng các tính năng mở rộng
```

**📍 Quan trọng:** Lần đầu cắm USB có thể mất 10-30 giây để service tìm file `CHC.CGO.Common.dll` trong thư mục cài đặt. Các lần sau sẽ nhanh hơn do đã cache đường dẫn.

**Kết thúc ngày làm việc:**
```
1. Đóng CHC Geomatics Office 2
2. Rút USB dongle
3. Các tính năng mở rộng tự động bị vô hiệu hóa
```

### Lưu ý quan trọng

⚠️ **KHÔNG được:**
- Copy nội dung USB sang USB khác
- Sao chép file `CHC.CGO.Common.dll` từ máy này sang máy khác
- Chỉnh sửa các file trong thư mục `dongle\`
- Rút USB khi đang sử dụng CHC Geomatics Office 2

✅ **Nên làm:**
- Giữ USB dongle an toàn
- Sao lưu dữ liệu công việc thường xuyên
- Rút USB khi đã đóng phần mềm

---

## 🔒 BẢO MẬT 4 LỚP

Hệ thống sử dụng 4 lớp bảo mật để chống sao chép:

**Layer 1: USB Hardware ID**
- USB được nhận dạng bằng số serial phần cứng duy nhất
- Copy sang USB khác → KHÔNG hoạt động

**Layer 2: Mã hóa AES-256**
- File DLL được mã hóa với khóa riêng của USB
- Không thể đọc hoặc giải mã bằng công cụ khác

**Layer 3: Machine Binding**
- DLL chỉ hoạt động trên máy đã đăng ký
- Copy sang máy khác → KHÔNG hoạt động

**Layer 4: Runtime Heartbeat**
- Kiểm tra USB mỗi 5 giây
- Rút USB → Tự động vô hiệu hóa trong 5 giây

---

## ❓ XỬ LÝ SỰ CỐ

### Vấn đề 1: Cắm USB nhưng không kích hoạt

**Triệu chứng:** Cắm USB, mở phần mềm nhưng không có tính năng mở rộng

**Giải pháp:**
1. Rút USB, đợi 10 giây, cắm lại
2. Kiểm tra USB có thư mục `dongle\` không
3. Kiểm tra service đang chạy:
   ```powershell
   Get-Service -Name "DongleSyncService"
   ```
4. Restart service:
   ```powershell
   Restart-Service -Name "DongleSyncService"
   ```

### Vấn đề 2: Tính năng không hoạt động sau khi rút USB

**Triệu chứng:** Rút USB nhưng vẫn thấy tính năng mở rộng

**Giải pháp:**
1. Đóng CHC Geomatics Office 2 hoàn toàn
2. Đợi 10 giây
3. Mở lại phần mềm
4. Tính năng mở rộng sẽ biến mất

### Vấn đề 3: Lỗi "Invalid machine binding"

**Triệu chứng:** Thông báo lỗi khi cắm USB

**Nguyên nhân:** USB đã được đăng ký trên máy khác

**Giải pháp:**
1. Liên hệ bộ phận IT để reset binding
2. KHÔNG tự ý copy file
3. Sử dụng USB đúng với máy đã đăng ký

### Vấn đề 4: Service không khởi động

**Giải pháp:**
1. Mở `services.msc`
2. Tìm "USB Dongle Sync Service"
3. Nhấp phải → Start
4. Nếu vẫn lỗi → Cài đặt lại installer

---

## 📊 LOGS VÀ GIÁM SÁT

### Vị trí log files

```
C:\ProgramData\DongleSyncService\logs\
  ├── service-20251203.log    (Log service)
  ├── patch-20251203.log      (Log DLL patch)
  └── ...
```

### Kiểm tra logs (cho IT)

```powershell
# Xem log mới nhất
Get-Content "C:\ProgramData\DongleSyncService\logs\service-*.log" -Tail 50

# Tìm lỗi
Select-String -Path "C:\ProgramData\DongleSyncService\logs\*.log" -Pattern "ERROR"
```

---

## 🔧 GỠ CÀI ĐẶT

### Cách 1: Qua Control Panel

1. Mở **Control Panel** → **Programs and Features**
2. Tìm "DongleSystem"
3. Nhấn **Uninstall**
4. Làm theo hướng dẫn

### Cách 2: Qua PowerShell

```powershell
# Xem danh sách installed
Get-WmiObject -Class Win32_Product | Where-Object {$_.Name -like "*Dongle*"}

# Gỡ cài đặt
$app = Get-WmiObject -Class Win32_Product | Where-Object {$_.Name -eq "DongleSystem"}
$app.Uninstall()
```

### Sau khi gỡ cài đặt

- CHC Geomatics Office 2 vẫn hoạt động bình thường
- Tính năng mở rộng không còn khả dụng
- File DLL gốc được khôi phục tự động

---

## 📞 HỖ TRỢ

### Khi nào cần liên hệ IT?

- USB dongle bị mất hoặc hỏng
- Cần đăng ký USB trên máy mới
- Lỗi không tự xử lý được
- Cần cài đặt trên nhiều máy

### Thông tin cần cung cấp khi báo lỗi

1. Mô tả chi tiết vấn đề
2. Thời điểm xảy ra lỗi
3. File log trong `C:\ProgramData\DongleSyncService\logs\`
4. Screenshot nếu có thông báo lỗi
5. Thông tin máy: Windows version, CHC Geomatics Office 2 version

---

## ✨ CÁC TÍNH NĂNG MỞ RỘNG

*(Tùy vào patch.dll được cung cấp)*

Ví dụ các tính năng có thể có:
- 📈 Báo cáo nâng cao
- 🗺️ Export định dạng đặc biệt
- 🔧 Công cụ tính toán chuyên dụng
- 📊 Phân tích dữ liệu mở rộng
- 🎨 Tùy chỉnh giao diện

*(Liên hệ bộ phận kỹ thuật để biết chi tiết tính năng)*

---

## 📝 CHANGELOG

**Version 1.0.0** (2025-12-03)
- Phát hành ban đầu
- Hỗ trợ CHC Geomatics Office 2
- 4 lớp bảo mật
- Tự động patch/restore
- Heartbeat monitor 5 giây

---

**© 2025 - Hệ thống USB Dongle cho CHC Geomatics Office 2**

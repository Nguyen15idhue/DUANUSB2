# 📦 SẢN PHẨM GIAO CHO NGƯỜI DÙNG CUỐI

## 🎁 DANH SÁCH SẢN PHẨM

### 1. **File Cài Đặt** (Bắt buộc)

```
📁 DongleSystem-v1.0.0/
  ├── DongleSystem-Installer.msi     (5-10 MB)
  └── README.txt                      (Hướng dẫn nhanh)
```

**Nội dung installer:**
- ✅ DongleSyncService.exe (Windows Service)
- ✅ Tất cả dependencies (DLLs)
- ✅ Tự động cài service và khởi động
- ✅ Tạo thư mục `C:\ProgramData\DongleSyncService`
- ✅ Tự động recovery nếu service crash

---

### 2. **USB Dongle** (Bắt buộc cho mỗi người dùng)

```
USB Drive:\
  └── dongle\
      ├── config.json          (200 bytes)
      ├── dongle.key           (100 bytes)
      ├── patch.dll.enc        (50 KB - 5 MB tùy DLL)
      ├── iv.bin               (16 bytes)
      └── README.txt           (1 KB)
```

**Đặc điểm USB:**
- USB thường (không cần USB chuyên dụng)
- Dung lượng tối thiểu: 64 MB
- Được tạo bằng `DongleCreatorTool`
- Mỗi USB là duy nhất (hardware ID)

---

### 3. **Tài Liệu** (Bắt buộc)

```
📁 Docs/
  ├── USER-GUIDE.md              (Hướng dẫn người dùng)
  ├── INSTALLATION.md            (Hướng dẫn cài đặt)
  ├── TROUBLESHOOTING.md         (Xử lý sự cố)
  └── FAQ.md                     (Câu hỏi thường gặp)
```

---

### 4. **Công Cụ Tạo USB** (Chỉ cho IT/Admin)

```
📁 DongleCreatorTool/
  ├── DongleCreatorTool.exe      (WinForms app)
  ├── PatchedDLL/
  │   └── CHC.CGO.Common.dll     (DLL đã tinh chỉnh)
  └── CREATOR-GUIDE.md           (Hướng dẫn tạo USB)
```

**Chỉ dành cho:**
- Nhân viên IT
- Administrator
- Technical support

---

## 🎯 QUY TRÌNH TRIỂN KHAI

### Bước 1: Chuẩn bị (IT Department)

1. **Build installer**
   ```powershell
   cd F:\3.Laptrinh\DUANUSB2\src
   dotnet publish DongleSyncService -c Release -o publish
   # Build MSI từ WiX
   candle Product.wxs -out Product.wixobj
   light Product.wixobj -o DongleSystem-Installer.msi
   ```

2. **Chuẩn bị DLL patch**
   - Lấy file `CHC.CGO.Common.dll` gốc từ CHC Geomatics Office 2
   - Tinh chỉnh/patch DLL (thêm features)
   - Test DLL patch hoạt động

3. **Tạo USB dongles**
   - Mở `DongleCreatorTool.exe`
   - Chọn USB
   - Chọn `CHC.CGO.Common.dll` đã patch
   - Nhấn "Create Dongle"
   - Lặp lại cho số lượng USB cần thiết

4. **QA Testing**
   - Test trên máy sạch (fresh Windows)
   - Test cắm/rút USB
   - Test copy USB (phải FAIL)
   - Test copy DLL sang máy khác (phải FAIL)

---

### Bước 2: Phân phối cho người dùng

**Mỗi người dùng nhận:**
```
✅ 1x DongleSystem-Installer.msi
✅ 1x USB Dongle (riêng biệt)
✅ 1x USER-GUIDE.md
✅ 1x Hotline/Email hỗ trợ
```

---

### Bước 3: Hướng dẫn người dùng cài đặt

**Gửi email hướng dẫn:**

```
Subject: Hướng dẫn cài đặt USB Dongle - CHC Geomatics Office 2

Kính gửi [Tên],

Bạn đã nhận được:
1. File cài đặt: DongleSystem-Installer.msi
2. USB Dongle số #12345
3. Tài liệu hướng dẫn

BƯỚC CÀI ĐẶT:
1. Nhấp đúp DongleSystem-Installer.msi
2. Làm theo hướng dẫn (Next → Next → Install)
3. Cắm USB Dongle vào máy
4. Mở CHC Geomatics Office 2
5. Kiểm tra các tính năng mới

Lưu ý:
- Không chia sẻ USB cho người khác
- Không copy nội dung USB
- Rút USB khi không sử dụng

Hỗ trợ: support@company.com | Hotline: 1900-xxxx

Trân trọng,
IT Department
```

---

## 📋 CHECKLIST GIAO HÀNG

### Cho mỗi người dùng (End User)

- [ ] File `DongleSystem-Installer.msi`
- [ ] USB Dongle đã tạo và test
- [ ] Tài liệu `USER-GUIDE.md` (PDF hoặc Word)
- [ ] Email hướng dẫn cài đặt
- [ ] Thông tin liên hệ hỗ trợ

### Cho IT Department

- [ ] Source code (nếu cần)
- [ ] `DongleCreatorTool.exe`
- [ ] DLL patch gốc (`CHC.CGO.Common.dll`)
- [ ] Tài liệu kỹ thuật (Day 1-6 docs)
- [ ] Script backup/restore
- [ ] Danh sách USB đã tạo (số serial, người dùng)

---

## 🔧 CÔNG CỤ TẠO USB (CHO IT)

### DongleCreatorTool.exe

**Giao diện:**
```
┌─────────────────────────────────────────┐
│  USB Dongle Creator Tool                │
├─────────────────────────────────────────┤
│                                          │
│  USB Drive:    [E:\          ▼] Refresh │
│                                          │
│  Patch DLL:    [Browse for DLL...]      │
│  Path: C:\...\CHC.CGO.Common.dll        │
│                                          │
│           [Create Dongle]                │
│                                          │
│  Status: Ready                           │
└─────────────────────────────────────────┘
```

**Workflow:**
1. Cắm USB trống vào máy
2. Chạy `DongleCreatorTool.exe` với quyền Admin
3. Chọn ổ USB
4. Chọn file `CHC.CGO.Common.dll` đã patch
5. Nhấn "Create Dongle"
6. Đợi 5-10 giây
7. Thông báo thành công → Gắn nhãn USB với số serial
8. Lưu thông tin USB vào database/spreadsheet

**Log tạo USB:**
```csv
USB_Serial,Created_Date,Created_By,User_Assigned,Status
ABC123XYZ,2025-12-03,admin@company.com,user1@company.com,Active
DEF456UVW,2025-12-03,admin@company.com,user2@company.com,Active
```

---

## 💾 BACKUP VÀ KHÔI PHỤC

### Backup (IT phải làm)

**Backup installer:**
```powershell
# Copy installer vào network share
Copy-Item "DongleSystem-Installer.msi" "\\server\software\DongleSystem\"

# Backup source code
git tag v1.0.0
git push --tags
```

**Backup DLL gốc:**
```powershell
# Backup DLL patch
Copy-Item "CHC.CGO.Common.dll" "\\server\backups\DLLPatches\v1.0.0\"
```

### Khôi phục người dùng

**Nếu người dùng mất USB:**
1. IT tạo USB mới từ `DongleCreatorTool`
2. Người dùng phải cài lại service (để reset machine binding)
3. Cắm USB mới → Tự động bind với máy

**Nếu service bị lỗi:**
```powershell
# Gỡ cài đặt
msiexec /x DongleSystem-Installer.msi

# Xóa data cũ (nếu cần)
Remove-Item "C:\ProgramData\DongleSyncService" -Recurse -Force

# Cài lại
msiexec /i DongleSystem-Installer.msi /qn
```

---

## 📊 THEO DÕI TRIỂN KHAI

### Database theo dõi USB

```sql
CREATE TABLE USB_Dongles (
    USB_Serial VARCHAR(50) PRIMARY KEY,
    Created_Date DATETIME,
    Created_By VARCHAR(100),
    User_Email VARCHAR(100),
    User_Name VARCHAR(100),
    Machine_Name VARCHAR(100),
    Status VARCHAR(20), -- Active, Lost, Replaced
    Notes TEXT
);
```

### Dashboard

**Metrics cần theo dõi:**
- Số USB đã tạo
- Số người dùng active
- Số sự cố hỗ trợ
- Số USB bị mất
- Service uptime

---

## 🎓 TRAINING CHO IT

### Nội dung training

1. **Kiến thức nền:**
   - 4 lớp bảo mật hoạt động như thế nào
   - USB hardware ID
   - Machine binding

2. **Thực hành:**
   - Tạo USB dongle
   - Cài đặt trên máy test
   - Xử lý sự cố phổ biến
   - Đọc logs

3. **Tools:**
   - DongleCreatorTool
   - Services.msc
   - Event Viewer
   - PowerShell scripts

---

## 📞 SUPPORT WORKFLOW

### Level 1 (User tự xử lý)
- Đọc USER-GUIDE.md
- Đọc TROUBLESHOOTING.md
- Restart service
- Rút/cắm lại USB

### Level 2 (IT Support)
- Kiểm tra logs
- Restart service từ xa
- Reset machine binding
- Tạo USB mới nếu cần

### Level 3 (Developer)
- Bugs trong code
- Cập nhật DLL patch
- Cập nhật service
- Release bản mới

---

## ✅ ACCEPTANCE CRITERIA

### Trước khi giao cho người dùng:

- [ ] Installer chạy không lỗi trên Windows 10/11
- [ ] Service tự động start sau khi cài
- [ ] Cắm USB → DLL được patch thành công
- [ ] Rút USB → DLL được restore tự động
- [ ] Copy USB → FAIL (hardware ID khác)
- [ ] Copy DLL sang máy khác → FAIL (machine binding)
- [ ] Logs ghi đầy đủ (không có ERROR)
- [ ] Uninstaller hoạt động (DLL gốc được khôi phục)
- [ ] Tài liệu đầy đủ và rõ ràng

---

**🎉 Hoàn tất - Sẵn sàng giao cho người dùng!**

# 🎯 INNO SETUP INSTALLER GUIDE

## Professional GUI Installer for Dongle Sync Service

Thay vì MSI (có lỗi) hoặc PowerShell (cần Run as Admin thủ công), chúng ta sử dụng **Inno Setup** để tạo một **EXE installer chuyên nghiệp** với GUI đẹp, dễ sử dụng.

---

## ✨ Tính Năng Installer

### 🎨 Giao diện GUI Chuyên Nghiệp
- **Modern wizard style** (giống như các phần mềm thương mại)
- **Progress bar** hiển thị tiến trình cài đặt
- **Welcome screen, License agreement, Install location**
- **Finish page** với hướng dẫn tiếp theo

### 🔍 Kiểm Tra Thông Minh
- **Tự động phát hiện CHC Geomatics Office 2**
  - Kiểm tra 3 vị trí cài đặt phổ biến
  - Hiển thị cảnh báo nếu chưa cài app CHC
  - Vẫn cho phép cài service (để cài sau)

- **Xử lý Service Cũ**
  - Tự động phát hiện service đang chạy
  - Dừng service cũ trước khi cài
  - Xóa service cũ và cài mới

### 🚀 Cài Đặt Tự Động
- **Tự động tạo thư mục**: `C:\Program Files\CHC Geomatics\Dongle Service`
- **Tự động tạo data folders**: `C:\ProgramData\DongleSyncService\{backups, logs}`
- **Tự động cài đặt Windows Service** với quyền LocalSystem
- **Tự động khởi động service** sau khi cài
- **Cấu hình service recovery**: Tự động restart nếu crash

### 📁 Shortcuts Tự Động
- **Start Menu**:
  - View Service Logs
  - Service Manager (services.msc)
  - Uninstall
- **Desktop** (tùy chọn): Shortcut mở log file

### 🗑️ Gỡ Cài Đặt Thông Minh
- **Tự động dừng service** trước khi xóa
- **Xóa service** khỏi Windows
- **Xóa toàn bộ files** (giữ lại logs nếu muốn)
- **Add/Remove Programs** tích hợp

---

## 📥 Bước 1: Cài Đặt Inno Setup

### Download
1. Truy cập: **https://jrsoftware.org/isdl.php**
2. Download: **innosetup-6.3.3.exe** (hoặc phiên bản mới nhất)
3. Chọn **Unicode version** (không phải ANSI)

### Cài Đặt
1. Chạy file `innosetup-6.3.3.exe`
2. Next → Next → Install
3. Đường dẫn mặc định: `C:\Program Files (x86)\Inno Setup 6\`
4. Finish

### Kiểm Tra
```powershell
Test-Path "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
# Should return: True
```

---

## 🔨 Bước 2: Build Installer

### Option 1: Build Tự Động (Khuyến Nghị)
```powershell
cd F:\3.Laptrinh\DUANUSB2\scripts
.\build-installer.ps1
```

Script sẽ:
1. ✅ Kiểm tra Inno Setup đã cài chưa
2. 🔨 Build DongleSyncService (Release, win-x64, self-contained)
3. 📋 Verify tất cả files cần thiết
4. 🚀 Compile Inno Setup installer
5. ✅ Tạo file `DongleSyncService-Setup-v1.0.0.exe` trong `output/`

### Option 2: Skip Build (Dùng Binary Sẵn Có)
```powershell
.\build-installer.ps1 -SkipBuild
```

### Option 3: Build + Mở Folder Output
```powershell
.\build-installer.ps1 -OpenOutput
```

---

## 📦 Bước 3: Kết Quả

### Output File
```
F:\3.Laptrinh\DUANUSB2\output\DongleSyncService-Setup-v1.0.0.exe
```

**File size**: ~60-80 MB (self-contained, không cần cài .NET)

### Thông Tin Installer
- **App Name**: USB Dongle Sync Service
- **Version**: 1.0.0
- **Publisher**: CHC Geomatics
- **GUID**: `{8DB3F8A4-8021-4473-868A-A53BB2E39759}`

---

## 🧪 Bước 4: Test Installer

### Cài Đặt Thử
1. **Right-click** file `DongleSyncService-Setup-v1.0.0.exe`
2. Select **"Run as administrator"**
3. Follow wizard:
   - Welcome → Next
   - License Agreement → Accept
   - CHC App Detection → (xem kết quả kiểm tra)
   - Destination Folder → Next (hoặc chọn folder khác)
   - Ready to Install → Install
   - Installing... (progress bar)
   - Finish

### Kiểm Tra Sau Khi Cài
```powershell
# Check service status
Get-Service DongleSyncService

# Check service configuration
sc.exe qc DongleSyncService

# View logs
Get-Content C:\ProgramData\DongleSyncService\logs\service-*.log -Tail 20

# Check installed files
Get-ChildItem "C:\Program Files\CHC Geomatics\Dongle Service" -Recurse
```

### Gỡ Cài Đặt
**Option 1**: Control Panel → Programs and Features → Uninstall
**Option 2**: Start Menu → CHC Geomatics → Uninstall USB Dongle Sync Service

---

## 📋 Bước 5: Distribution (Gửi Cho Khách)

### File Cần Gửi
```
DongleSyncService-Setup-v1.0.0.exe  (Installer)
DongleCreatorTool.exe               (Tạo USB dongle)
README.md                           (Hướng dẫn)
```

### Hướng Dẫn Cho Khách
1. **Cài CHC Geomatics Office 2** (nếu chưa có)
2. **Right-click installer** → Run as administrator
3. **Follow wizard** → Next → Next → Install → Finish
4. **Tạo USB dongle** bằng DongleCreatorTool
5. **Cắm dongle** → Service sẽ tự động patch DLL

---

## 🔧 Customization (Tùy Chỉnh)

### Thay Đổi Icon
Thay file: `installer\icon.ico`

### Thay Đổi License
Chỉnh file: `installer\License.rtf`

### Thay Đổi Version
Edit file: `installer\DongleSyncService-Setup.iss`
```iss
#define MyAppVersion "1.0.1"  ; Thay đổi ở đây
```

### Thêm Files
Edit file: `installer\DongleSyncService-Setup.iss`
```iss
[Files]
Source: "path\to\your\file"; DestDir: "{app}"; Flags: ignoreversion
```

---

## 🐛 Troubleshooting

### Lỗi: "Inno Setup not found"
- Cài Inno Setup từ: https://jrsoftware.org/isdl.php
- Kiểm tra đường dẫn: `C:\Program Files (x86)\Inno Setup 6\ISCC.exe`

### Lỗi: "Service Executable not found"
- Build project trước: `dotnet publish -c Release -r win-x64 --self-contained true`
- Hoặc dùng: `.\build-installer.ps1` (không dùng `-SkipBuild`)

### Lỗi: "DLLPatch.dll not found"
- Kiểm tra file: `src\DongleSyncService\bin\Release\net8.0\win-x64\publish\DLLPatch.dll`
- Rebuild DLLPatch project: `dotnet build -c Release src\DLLPatch`

### Lỗi: "License.rtf not found"
- Copy từ: `deployment\DongleSyncSystem-v1.0.0\Installer\License.rtf`
- Đến: `installer\License.rtf`

### Installer Không Chạy
- Right-click → Properties → Unblock (nếu download từ internet)
- Chạy với quyền Administrator

---

## 📚 Technical Details

### Inno Setup Features Used
- **Modern wizard style** - Giao diện đẹp, chuyên nghiệp
- **Pascal scripting** - Logic phức tạp (check CHC app, stop service)
- **Custom pages** - Trang hiển thị kết quả kiểm tra
- **Service installation** - `sc.exe` commands
- **Registry operations** - Add/Remove Programs integration
- **File compression** - LZMA2 (tối ưu size)

### Why Inno Setup?
✅ **Miễn phí** & open source
✅ **Nhẹ** (~3MB installer builder)
✅ **Mạnh mẽ** - Dùng bởi nhiều phần mềm thương mại
✅ **Dễ customize** - Pascal scripting
✅ **Professional UI** - Không thua WiX, NSIS
✅ **No dependencies** - Tạo standalone EXE
✅ **Code signing** support - Có thể ký digital signature

### Why NOT MSI?
❌ WiX phức tạp, khó debug
❌ Error 1920 service timeout
❌ Rollback mechanism phức tạp
❌ Khó customize UI
❌ Build time lâu

### Why NOT PowerShell Installer?
❌ Cần Run as Admin thủ công
❌ Không có GUI
❌ Execution policy issues
❌ Không có Add/Remove Programs entry
❌ Khó gửi cho khách (nhiều files)

---

## 📝 Files Structure

```
installer/
├── DongleSyncService-Setup.iss    # Inno Setup script (MAIN FILE)
├── icon.ico                       # App icon
├── License.rtf                    # License agreement
└── ...

scripts/
├── build-installer.ps1            # Auto build script (MAIN SCRIPT)
└── ...

output/
└── DongleSyncService-Setup-v1.0.0.exe  # Final installer (DISTRIBUTE THIS)
```

---

## 🎯 Next Steps

1. ✅ Cài Inno Setup
2. ✅ Chạy `.\build-installer.ps1`
3. ✅ Test installer
4. ✅ Gửi cho khách: `DongleSyncService-Setup-v1.0.0.exe`
5. ✅ Khách double-click → Next → Next → Finish → Done! 🎉

---

## 💡 Tips

### Faster Build (Development)
```powershell
# Build service once
dotnet publish -c Release -r win-x64 --self-contained true

# Then build installer multiple times (skip rebuild)
.\build-installer.ps1 -SkipBuild
```

### Test Without Install
Inno Setup có preview mode, nhưng thường không cần vì cài/gỡ rất nhanh.

### Auto-Update
Có thể thêm update checker vào service sau này, hoặc dùng GitHub Releases.

---

**🎊 DONE! Bây giờ bạn có một installer chuyên nghiệp, sang trọng, dễ gửi cho khách hàng!**

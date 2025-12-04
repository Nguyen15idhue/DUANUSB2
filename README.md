# USB Dongle Sync Service

Hệ thống quản lý license phần mềm CHC Geomatics Office 2 thông qua USB Dongle.

## 📋 Tổng Quan

Dự án bao gồm 2 phần chính:
- **DongleCreatorTool**: Tạo USB Dongle (dành cho DEV/ADMIN)
- **DongleSyncService**: Windows Service tự động patch/restore DLL (dành cho USER)

## 🚀 Quick Start

### Cho DEV - Tạo USB Dongle
```powershell
# 1. Publish Creator Tool
.\scripts\publish-dongle-creator.ps1

# 2. Chạy DongleCreatorTool.exe
# 3. Browse chọn DLL gốc (287 KB)
# 4. Chọn USB drive → Create Dongle
```

### Cho USER - Cài Đặt Service
```powershell
# 1. Build installer
.\scripts\build-installer.ps1

# 2. Chạy installer (Run as Admin)
.\output\DongleSyncService-Setup-v1.0.0.exe

# 3. Cắm USB → Mở CHC → Done!
```

## 📂 Cấu Trúc Dự Án

```
DUANUSB2/
├── src/                              # Source code
│   ├── DongleCreatorTool/           # Tool tạo USB dongle
│   ├── DongleSyncService/           # Windows Service
│   └── DLLPatch/                    # DLL được inject vào CHC
├── installer/                        # Inno Setup installer
│   ├── DongleSyncService-Setup.iss  # Script chính
│   ├── icon.ico                     # Icon installer
│   └── License.rtf                  # License agreement
├── scripts/                          # Build scripts
│   ├── build-installer.ps1          # Build Inno Setup installer
│   └── publish-dongle-creator.ps1   # Publish Creator Tool
├── docs/                             # Tài liệu
│   ├── QUICK-GUIDE.md               # ⭐ Hướng dẫn nhanh
│   ├── TROUBLESHOOTING.md           # ⭐ Xử lý sự cố
│   ├── BUILD-PRODUCTION.md          # Build production
│   ├── INNO-SETUP-INSTALLER.md      # Chi tiết Inno Setup
│   └── ...
└── output/                           # Build output
    ├── DongleSyncService-Setup-v1.0.0.exe  # Installer
    └── DongleCreatorTool/           # Creator tool binaries
```

## 📖 Tài Liệu

### Người Dùng
- **[QUICK-GUIDE.md](docs/QUICK-GUIDE.md)** - Hướng dẫn nhanh (DEV + USER)
- **[TROUBLESHOOTING.md](docs/TROUBLESHOOTING.md)** - Xử lý 9 nhóm lỗi
- **[USER-GUIDE.md](docs/USER-GUIDE.md)** - Hướng dẫn chi tiết end-user

### Developer
- **[BUILD-PRODUCTION.md](docs/BUILD-PRODUCTION.md)** - Build & deploy production
- **[INNO-SETUP-INSTALLER.md](docs/INNO-SETUP-INSTALLER.md)** - Chi tiết installer
- **[DEV-TESTING-COMMANDS.md](docs/DEV-TESTING-COMMANDS.md)** - Lệnh test nhanh
- **[TECHNICAL-OVERVIEW.md](docs/TECHNICAL-OVERVIEW.md)** - Kiến trúc hệ thống
- **[NGUYEN-LY-HOAT-DONG.md](docs/NGUYEN-LY-HOAT-DONG.md)** - Nguyên lý hoạt động

## 🔧 Yêu Cầu Hệ Thống

### Development
- Windows 10/11
- .NET 8.0 SDK
- Visual Studio 2022 (hoặc VS Code)
- Inno Setup 6.6.1+
- PowerShell 5.1+

### Production (End User)
- Windows 10/11
- Quyền Administrator (chỉ khi cài đặt)
- CHC Geomatics Office 2
- USB Drive (≥4GB)

## 🛠️ Build Instructions

### 1. Build Service Installer
```powershell
# Build service và tạo installer
.\scripts\build-installer.ps1

# Hoặc skip build nếu đã build rồi
.\scripts\build-installer.ps1 -SkipBuild
```

Output: `output\DongleSyncService-Setup-v1.0.0.exe` (25 MB)

### 2. Build Creator Tool
```powershell
# Publish Creator Tool
.\scripts\publish-dongle-creator.ps1
```

Output: `output\DongleCreatorTool\DongleCreatorTool.exe`

## 🧪 Testing

### Test Service Locally
```powershell
# Build và publish service
cd src\DongleSyncService
dotnet publish -c Release -r win-x64 --self-contained

# Copy files thủ công hoặc dùng installer
```

### Test Creator Tool
```powershell
# Chạy từ output folder
.\output\DongleCreatorTool\DongleCreatorTool.exe
```

## 📦 Deployment

### Giao cho User
1. File: `output\DongleSyncService-Setup-v1.0.0.exe`
2. Docs: `QUICK-GUIDE.md`, `TROUBLESHOOTING.md`
3. Hướng dẫn: Chuột phải → Run as administrator → Next → Next → Finish

### Giao cho DEV/Admin
1. Tool: `output\DongleCreatorTool\`
2. Docs: `QUICK-GUIDE.md` (Phần A)
3. File DLL gốc: `CHC.CGO.Common.dll` (287 KB)

## 🔍 Troubleshooting

Xem chi tiết trong [TROUBLESHOOTING.md](docs/TROUBLESHOOTING.md)

**Lỗi phổ biến:**

| Vấn Đề | Giải Pháp |
|--------|-----------|
| Windows SmartScreen chặn | Click "More info" → "Run anyway" |
| Antivirus xóa file | Tắt tạm → Add exclusion → Cài lại |
| Service không chạy | `Start-Service DongleSyncService` |
| Cắm USB không hoạt động | Check USB có folder `dongle\` với 3 files |
| Access Denied | Bỏ ReadOnly attribute của DLL |

## 🔐 Security

- AES-256-CBC encryption cho DLL
- Machine binding optional (MAC address + CPU ID)
- Service chạy với LocalSystem account
- 3-layer backup integrity check (size + SHA256 + metadata)
- Tự động remove ReadOnly attribute trước khi patch

## 📊 Key Features

### DongleCreatorTool
✅ Encrypt DLL với AES-256  
✅ Generate random IV và Key  
✅ Machine binding (optional)  
✅ Verify dongle sau khi tạo  
✅ GUI dễ sử dụng  

### DongleSyncService
✅ Auto-detect USB insert/remove (WMI)  
✅ Auto-patch DLL khi cắm USB  
✅ Auto-restore DLL khi rút USB  
✅ Heartbeat monitor (1s interval)  
✅ Backup với metadata validation  
✅ ReadOnly attribute handling  
✅ Detailed logging (Serilog)  
✅ IPC Server cho DLLPatch communication  

## 🎯 Workflow

```
1. DEV: Tạo USB Dongle
   DongleCreatorTool → Chọn DLL gốc → Create → USB có 3 files

2. USER: Cài Service
   Installer → Next Next Finish → Service tự động chạy

3. USER: Sử Dụng
   Cắm USB → DLL patched → Mở CHC → Hoạt động ✅
   Rút USB → DLL restored → CHC báo lỗi license ✅
```

## 📝 Change Log

Xem [CHANGELOG.md](CHANGELOG.md)

### Latest (v1.0.0)
- ✅ Inno Setup installer (thay thế MSI)
- ✅ Auto-start service sau cài đặt
- ✅ ReadOnly attribute handling
- ✅ Comprehensive troubleshooting docs
- ✅ Quick guide cho DEV + USER

## 👥 Contributors

- **Nguyen15idhue** - Initial work

## 📄 License

Copyright © 2024-2025. All rights reserved.

## 🆘 Support

Gặp vấn đề? Xem:
1. [QUICK-GUIDE.md](docs/QUICK-GUIDE.md) - Hướng dẫn cơ bản
2. [TROUBLESHOOTING.md](docs/TROUBLESHOOTING.md) - 20+ kịch bản xử lý lỗi
3. [Issues](https://github.com/Nguyen15idhue/DUANUSB2/issues) - Báo cáo lỗi

---

**Made with ❤️ for CHC Geomatics Office 2 users**

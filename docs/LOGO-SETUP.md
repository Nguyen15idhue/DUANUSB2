# Hướng dẫn sử dụng Logo cho Ứng dụng

## Tổng quan

Hệ thống hỗ trợ 2 logo riêng biệt:
- **Logo MSI/Service**: Dùng cho Windows Service và MSI Installer
- **Logo Dev Tool**: Dùng cho DongleCreatorTool (công cụ tạo dongle)

## Đã hoàn thành ✓

### 1. Convert PNG sang ICO
✅ File icon đã được tạo:
- `src/DongleSyncService/icon.ico` - Icon cho Service/MSI
- `src/DongleCreatorTool/icon.ico` - Icon cho Dev Tool

### 2. Cấu hình Project Files
✅ Đã thêm `<ApplicationIcon>icon.ico</ApplicationIcon>` vào:
- `DongleSyncService.csproj`
- `DongleCreatorTool.csproj`

### 3. Test Build
✅ Build thành công với icon embedded

## Cách sử dụng Logo

### Chuẩn bị file PNG

Đặt file PNG vào thư mục gốc project:
```
F:\3.Laptrinh\DUANUSB2\
├── Treetog-I-Image-File.256.png   (logo cho cả 2 app)
```

**Yêu cầu:**
- Format: PNG 256x256 pixels
- Nên dùng background trong suốt
- File size: < 500KB

### Chạy script convert

```powershell
cd F:\3.Laptrinh\DUANUSB2
.\scripts\convert-logos-to-ico.ps1
```

Script sẽ:
1. Load PNG từ thư mục gốc
2. Resize thành 256x256 (nếu cần)
3. Convert sang ICO format
4. Lưu vào `src/DongleSyncService/icon.ico` và `src/DongleCreatorTool/icon.ico`

### Build ứng dụng với icon

#### Option 1: Build riêng lẻ

**DongleSyncService:**
```powershell
cd F:\3.Laptrinh\DUANUSB2\src\DongleSyncService
dotnet build -c Release
```

**DongleCreatorTool:**
```powershell
cd F:\3.Laptrinh\DUANUSB2\src\DongleCreatorTool
dotnet build -c Release
```

#### Option 2: Build tất cả (Khuyến nghị)

```powershell
cd F:\3.Laptrinh\DUANUSB2
.\scripts\build-all.ps1
```

## Kết quả

### Logo xuất hiện ở đâu?

#### 1. DongleSyncService (MSI Installer)
- ✅ Icon MSI installer trong Control Panel
- ✅ Icon trong Add/Remove Programs
- ✅ Icon file DongleSyncService.exe trong Task Manager
- ✅ Icon trong Start Menu shortcuts

#### 2. DongleCreatorTool
- ✅ Icon file DongleCreatorTool.exe
- ✅ Icon khi chạy ứng dụng (taskbar, window)
- ✅ Icon trong File Explorer

### Xác minh icon đã được embed

**Kiểm tra EXE file:**
```powershell
# Check DongleSyncService
Get-Item "F:\3.Laptrinh\DUANUSB2\src\DongleSyncService\bin\Release\net8.0\DongleSyncService.exe" | Select-Object -ExpandProperty VersionInfo

# Check DongleCreatorTool
Get-Item "F:\3.Laptrinh\DUANUSB2\src\DongleCreatorTool\bin\Release\net8.0-windows\DongleCreatorTool.exe" | Select-Object -ExpandProperty VersionInfo
```

**Xem icon trong File Explorer:**
- Mở File Explorer
- Navigate đến thư mục bin\Release
- Icon sẽ hiển thị trên file .exe

## Thay đổi Logo

### Cách 1: Thay file PNG và rebuild

1. **Thay file PNG mới:**
   ```powershell
   # Copy logo mới vào thư mục gốc
   Copy-Item "đường_dẫn_logo_mới.png" "F:\3.Laptrinh\DUANUSB2\Treetog-I-Image-File.256.png" -Force
   ```

2. **Convert lại:**
   ```powershell
   .\scripts\convert-logos-to-ico.ps1
   ```

3. **Rebuild:**
   ```powershell
   .\scripts\build-all.ps1
   ```

### Cách 2: Thay trực tiếp file ICO

Nếu bạn đã có file .ico sẵn:

```powershell
# Thay icon cho Service
Copy-Item "logo_service.ico" "F:\3.Laptrinh\DUANUSB2\src\DongleSyncService\icon.ico" -Force

# Thay icon cho Dev Tool
Copy-Item "logo_dev.ico" "F:\3.Laptrinh\DUANUSB2\src\DongleCreatorTool\icon.ico" -Force

# Rebuild
.\scripts\build-all.ps1
```

## Logo trong MSI Installer

Logo sẽ tự động được WiX Toolset sử dụng khi build MSI:

```xml
<!-- Product.wxs đã cấu hình -->
<Icon Id="icon.ico" SourceFile="$(var.SolutionDir)src\DongleSyncService\icon.ico" />
<Property Id="ARPPRODUCTICON" Value="icon.ico" />
```

Build MSI:
```powershell
.\scripts\build-msi.ps1
```

Logo sẽ xuất hiện:
- ✅ MSI installer dialog
- ✅ Control Panel > Programs and Features
- ✅ Windows Settings > Apps & Features

## Troubleshooting

### Icon không hiển thị sau khi build

**Nguyên nhân:** Windows cache icon cũ

**Giải pháp:**
```powershell
# Xóa icon cache của Windows
ie4uinit.exe -show

# Hoặc restart Explorer
Stop-Process -Name explorer -Force
```

### Script convert lỗi

**Lỗi:** "System.Drawing assembly not found"

**Giải pháp:**
```powershell
# Install .NET Desktop Runtime
winget install Microsoft.DotNet.DesktopRuntime.8
```

### Icon bị mờ hoặc vỡ

**Nguyên nhân:** PNG nguồn có độ phân giải thấp

**Giải pháp:**
- Dùng PNG 256x256 trở lên
- Dùng vector image nếu có
- Convert online: https://www.icoconverter.com/

### Build thành công nhưng ICO không embed vào EXE

**Kiểm tra:**
```powershell
# Xem .csproj có ApplicationIcon chưa
Get-Content "src\DongleSyncService\DongleSyncService.csproj" | Select-String "ApplicationIcon"
```

**Nếu không có, thêm thủ công:**
```xml
<PropertyGroup>
  <ApplicationIcon>icon.ico</ApplicationIcon>
</PropertyGroup>
```

## Scripts liên quan

### convert-logos-to-ico.ps1
- Chuyển PNG → ICO
- Resize về 256x256
- Lưu vào thư mục projects

### build-all.ps1
- Build Service + DongleCreatorTool
- Build MSI installer
- Tạo deployment package
- **Icon tự động được bao gồm**

## Lưu ý quan trọng

1. **File size:** ICO file nên < 100KB để không làm phình EXE
2. **Multi-size icon:** ICO nên chứa nhiều kích thước (16x16, 32x32, 48x48, 256x256)
3. **Transparency:** Dùng alpha channel cho background trong suốt
4. **Rebuild:** Mỗi lần thay icon phải rebuild để embed vào EXE

## Checklist hoàn chỉnh

- [x] File PNG 256x256 đã chuẩn bị
- [x] Chạy convert-logos-to-ico.ps1
- [x] .csproj có ApplicationIcon
- [x] Build Release successful
- [x] Icon hiển thị trong File Explorer
- [x] MSI installer có icon
- [x] Deployed app có icon

---

**Tóm tắt:** Logo đã được cấu hình đầy đủ. Mỗi lần build, icon sẽ tự động embed vào EXE và MSI installer. 🎨

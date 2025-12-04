# 🔨 Hướng Dẫn Build & Deploy

Hướng dẫn nhanh để build lại toàn bộ hệ thống và tạo file MSI installer mới.

---

## 📋 Yêu Cầu Trước Khi Build

### Phần Mềm Cần Thiết
- ✅ **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- ✅ **WiX Toolset v3.11** - [Download](https://wixtoolset.org/releases/)
- ✅ **Visual Studio 2022** (optional, để debug)
- ✅ **PowerShell 5.1+** (có sẵn trên Windows 10/11)

### Kiểm Tra Môi Trường
```powershell
# Check .NET SDK
dotnet --version
# Expected: 8.0.x hoặc cao hơn

# Check WiX Toolset
candle.exe -?
# Should show WiX version 3.11.x

# Check Git (optional)
git --version
```

---

## 🏗️ Build Service (EXE)

### Bước 1: Build Service Binary
```powershell
# Di chuyển đến thư mục project
cd F:\3.Laptrinh\DUANUSB2\src\DongleSyncService

# Build Release version (self-contained, single-file)
dotnet publish -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:DebugType=None `
  -p:DebugSymbols=false
```

### Bước 2: Kiểm Tra Output
```powershell
# File EXE được tạo tại:
# F:\3.Laptrinh\DUANUSB2\src\DongleSyncService\bin\Release\net8.0\win-x64\publish\DongleSyncService.exe

# Check file size (should be ~66-70 MB)
Get-Item ".\bin\Release\net8.0\win-x64\publish\DongleSyncService.exe" | 
  Select-Object Name, @{N='Size (MB)';E={[math]::Round($_.Length/1MB, 2)}}, LastWriteTime
```

**✅ Expected Output:**
```
Name                  Size (MB) LastWriteTime
----                  --------- -------------
DongleSyncService.exe 66.45     12/4/2025 12:24:37 PM
```

---

## 📦 Build MSI Installer

### Bước 1: Build WiX Installer
```powershell
# Di chuyển về root project
cd F:\3.Laptrinh\DUANUSB2

# Chạy script build MSI
& ".\scripts\build-msi.ps1"
```

### Bước 2: Kiểm Tra MSI Output
```powershell
# MSI được tạo tại:
# F:\3.Laptrinh\DUANUSB2\installer\bin\DongleSyncService-Setup.msi

Get-Item ".\installer\bin\DongleSyncService-Setup.msi" | 
  Select-Object Name, @{N='Size (MB)';E={[math]::Round($_.Length/1MB, 2)}}, LastWriteTime
```

**✅ Expected Output:**
```
Name                           Size (MB) LastWriteTime
----                           --------- -------------
DongleSyncService-Setup.msi    29.23     12/4/2025 12:27:10 PM
```

---

## 🚀 Deploy & Test

### Cài Đặt MSI Mới
```powershell
# Install/Upgrade service (yêu cầu Administrator)
Start-Process "msiexec.exe" -ArgumentList "/i `"F:\3.Laptrinh\DUANUSB2\installer\bin\DongleSyncService-Setup.msi`" /qn" -Wait -Verb RunAs

# Đợi 3 giây
Start-Sleep 3

# Kiểm tra service status
Get-Service DongleSyncService | Format-List Name, Status, StartType
```

### Nếu Service Stopped - Manual Fix
```powershell
# Chạy PowerShell AS ADMINISTRATOR

# Option 1: Start service trực tiếp
Start-Service DongleSyncService

# Option 2: Nếu start failed - Check binary path
sc.exe qc DongleSyncService | Select-String "BINARY_PATH"

# Option 3: Nếu path sai - Uninstall & Reinstall
sc.exe delete DongleSyncService
msiexec.exe /i "F:\3.Laptrinh\DUANUSB2\installer\bin\DongleSyncService-Setup.msi" /qn
Start-Service DongleSyncService
```

### Kiểm Tra Service Hoạt Động
```powershell
# Check service status
Get-Service DongleSyncService

# Check service logs
$logFile = Get-ChildItem "C:\ProgramData\DongleSyncService\logs\" -Filter "service-*.log" | 
  Sort-Object LastWriteTime -Descending | Select-Object -First 1
Get-Content $logFile.FullName -Tail 20

# Verify binary path
sc.exe qc DongleSyncService
```

**✅ Expected:**
- Status: **Running**
- StartType: **Automatic**
- BinaryPath: `"C:\Program Files\CHC Geomatics\Dongle Service\DongleSyncService.exe"`

---

## 🔧 Build DongleCreatorTool (Optional)

### Build Creator Tool GUI (Self-Contained - Recommended)
```powershell
cd F:\3.Laptrinh\DUANUSB2\src\DongleCreatorTool

# Build self-contained single EXE (ready to run)
dotnet publish -c Release -r win-x64

# Output tại:
# .\bin\Release\net8.0-windows\win-x64\publish\DongleCreatorTool.exe (~69 MB)
```

**Kết quả:**
- ✅ **68.65 MB** - Single EXE file với compression enabled
- ✅ **Self-contained** - Chạy ngay, không cần cài .NET Runtime
- ✅ **Ready to distribute** - Copy file là dùng được ngay

### Project Configuration
File `.csproj` đã được tối ưu với:
- `PublishSingleFile=true` - Gộp thành 1 file EXE duy nhất
- `EnableCompressionInSingleFile=true` - Nén để giảm size (165 MB → 69 MB)
- `IncludeNativeLibrariesForSelfExtract=true` - Tự động extract native DLLs
- `DebugSymbols=false` - Không bao gồm debug symbols

---

## 📝 Build All - One Command

### Script Tự Động Build Toàn Bộ
Tạo file `build-all.ps1`:

```powershell
# build-all.ps1
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Building All Components" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# 1. Build Service
Write-Host "[1/3] Building DongleSyncService..." -ForegroundColor Yellow
cd F:\3.Laptrinh\DUANUSB2\src\DongleSyncService
dotnet publish -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:DebugType=None -p:DebugSymbols=false -v q

if ($LASTEXITCODE -eq 0) {
    Write-Host "[OK] Service built successfully`n" -ForegroundColor Green
} else {
    Write-Host "[FAIL] Service build failed`n" -ForegroundColor Red
    exit 1
}

# 2. Build Creator Tool
Write-Host "[2/3] Building DongleCreatorTool..." -ForegroundColor Yellow
cd F:\3.Laptrinh\DUANUSB2\src\DongleCreatorTool
dotnet publish -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true -v q

if ($LASTEXITCODE -eq 0) {
    Write-Host "[OK] Creator Tool built successfully`n" -ForegroundColor Green
} else {
    Write-Host "[FAIL] Creator Tool build failed`n" -ForegroundColor Red
    exit 1
}

# 3. Build MSI
Write-Host "[3/3] Building MSI Installer..." -ForegroundColor Yellow
cd F:\3.Laptrinh\DUANUSB2
& ".\scripts\build-msi.ps1"

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "ALL BUILDS COMPLETED SUCCESSFULLY!" -ForegroundColor Green
    Write-Host "========================================`n" -ForegroundColor Green
    
    Write-Host "Build artifacts:" -ForegroundColor Cyan
    Write-Host "- Service EXE: src\DongleSyncService\bin\Release\net8.0\win-x64\publish\" -ForegroundColor White
    Write-Host "- Creator EXE: src\DongleCreatorTool\bin\Release\net8.0-windows\win-x64\publish\" -ForegroundColor White
    Write-Host "- MSI Installer: installer\bin\DongleSyncService-Setup.msi`n" -ForegroundColor White
    
    # Show file sizes
    $msi = Get-Item ".\installer\bin\DongleSyncService-Setup.msi"
    $svc = Get-Item ".\src\DongleSyncService\bin\Release\net8.0\win-x64\publish\DongleSyncService.exe"
    
    Write-Host "File sizes:" -ForegroundColor Cyan
    Write-Host ("MSI: {0:N2} MB" -f ($msi.Length / 1MB)) -ForegroundColor White
    Write-Host ("Service: {0:N2} MB" -f ($svc.Length / 1MB)) -ForegroundColor White
} else {
    Write-Host "[FAIL] MSI build failed" -ForegroundColor Red
    exit 1
}
```

### Chạy Build All
```powershell
cd F:\3.Laptrinh\DUANUSB2
& ".\build-all.ps1"
```

---

## 🐛 Troubleshooting

### Build Error: "SDK not found"
```powershell
# Install .NET 8.0 SDK
winget install Microsoft.DotNet.SDK.8
```

### Build Error: "candle.exe not found"
```powershell
# Add WiX to PATH
$env:Path += ";C:\Program Files (x86)\WiX Toolset v3.11\bin"
```

### MSI Build Error: "File not found"
```bash
# Đảm bảo Service đã được build trước
cd F:\3.Laptrinh\DUANUSB2\src\DongleSyncService
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Sau đó build MSI
cd F:\3.Laptrinh\DUANUSB2
& ".\scripts\build-msi.ps1"
```

### Service Won't Start After Install
```powershell
# Check Event Log
Get-EventLog -LogName Application -Source ".NET Runtime" -Newest 3

# Check binary path
sc.exe qc DongleSyncService

# Reinstall if needed
sc.exe delete DongleSyncService
msiexec.exe /i "installer\bin\DongleSyncService-Setup.msi" /qn
```

---

## 📊 Verify Build Quality

### Check Service Features
```powershell
# Run service directly to see config
& "F:\3.Laptrinh\DUANUSB2\src\DongleSyncService\bin\Release\net8.0\win-x64\publish\DongleSyncService.exe" --help
```

### Verify Security Features
Các tính năng trong build mới:
- ✅ **5-Layer Security** (Hardware ID, AES-256, Machine Binding, Heartbeat, DLL Integrity)
- ✅ **Heartbeat Interval:** 3 seconds (configurable)
- ✅ **DLL Integrity Check:** SHA-256 hash + timestamp verification
- ✅ **Auto-Restore:** Phát hiện tampering và restore DLL gốc tự động
- ✅ **Auto-Close App:** Graceful + Force kill when USB removed

### Test Installation
```powershell
# Install trên máy test
msiexec.exe /i "DongleSyncService-Setup.msi" /l*v install.log

# Check install log nếu có lỗi
notepad install.log
```

---

## 📅 Version History

| Version | Date | Changes |
|---------|------|---------|
| v1.0.1 | Dec 4, 2025 | Added DLL Integrity Check, 3s heartbeat |
| v1.0.0 | Nov 2025 | Initial release with 4-layer security |

---

## 📞 Support

**Developer:** CHC Geomatics Development Team  
**Build System:** .NET 8.0 + WiX v3.11  
**Platform:** Windows 10/11 (64-bit)

---

*Document này cung cấp hướng dẫn nhanh để rebuild toàn bộ hệ thống. Để biết chi tiết kỹ thuật, xem TECHNICAL-OVERVIEW.md*

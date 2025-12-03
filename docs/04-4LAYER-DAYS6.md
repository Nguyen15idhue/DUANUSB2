# 🚀 HỆ THỐNG USB DONGLE 4 LAYERS - NGÀY 6

## 🎯 MỤC TIÊU NGÀY 6
- Hoàn tất kiểm thử end-to-end (E2E)
- Tạo installer (WiX) để cài service và tool
- Tài liệu hóa quy trình release và rollback
- Kiểm tra developer mode và hướng dẫn debug

---

## ⏰ 09:00 - 11:00 | End-to-end Test Plan (2 giờ)

Mục tiêu: Xác minh toàn bộ luồng hoạt động:
- Tạo USB dongle bằng `DongleCreatorTool`
- Cắm USB → `DongleSyncService` nhận, validate, patch DLL của App X
- Chạy App X → tính năng mở rộng hoạt động
- Rút USB → `DongleSyncService` tự động restore DLL trong ~5s

Bước kiểm thử (copyable):

1. Chuẩn bị
   - Build tất cả projects:

```powershell
cd F:\3.Laptrinh\DUANUSB2\src
dotnet build
```

   - Mở PowerShell chạy service trong console để dễ đọc log:

```powershell
cd F:\3.Laptrinh\DUANUSB2\src\DongleSyncService
dotnet run
```

2. Tạo dongle (trên máy dev)
   - Mở `DongleCreatorTool`, chọn ổ USB (Removable), chọn file `patch.dll` (tinh chỉnh), nhấn `Create Dongle`.
   - Kiểm tra thư mục USB:\dongle có các file: `config.json`, `dongle.key`, `patch.dll.enc`, `iv.bin`, `README.txt`.

3. Test cắm USB
   - Cắm USB vào máy test.
   - Quan sát log `DongleSyncService`:
     - Detect USB inserted
     - Validate dongle
     - Create/validate binding
     - Patch DLL (backup created under `C:\ProgramData\DongleSyncService\backups`)
     - State updated: `state.json` -> `IsPatched=true`

4. Chạy CHC Geomatics Office 2
   - Mở CHC Geomatics Office 2, thực hiện feature được chèn bởi patch.
   - Kiểm tra logs (service + patch logs) để đảm bảo patch hoạt động.

5. Rút USB
   - Rút USB
   - Quan sát log: Heartbeat detect or USBRemoved event → restore DLL
   - Kiểm tra CHC Geomatics Office 2: tính năng tùy biến bị vô hiệu (DLL trở về bản cũ)

6. Edge cases
   - Copy dongle folder sang USB khác → validation phải FAIL
   - Copy files từ USB ra máy → decryption/validation phải FAIL (machine binding)
   - Restart service while patched → Heartbeat should detect missing USB on restart and not break (state persisted)

---

## ⏰ 11:00 - 13:00 | Automated Smoke Tests (2 giờ)

Gợi ý tạo script kiểm thử tự động (PowerShell) cho E2E:

```powershell
# Example pseudo script (requires admin privileges for service management and file operations)
# 1. Start service in console mode
cd F:\3.Laptrinh\DUANUSB2\src\DongleSyncService
Start-Process -NoNewWindow -FilePath dotnet -ArgumentList 'run' -PassThru

# 2. Wait for service ready
Start-Sleep -Seconds 3

# 3. Copy prepared dongle folder to a removable drive letter (E:)
# 4. Simulate USB insert (physically or mount VHD), then sleep
Start-Sleep -Seconds 5

# 5. Check logs under C:\ProgramData\DongleSyncService\logs for "DLL PATCHED" entries
# 6. Wait and then remove USB
Start-Sleep -Seconds 10
# 7. Check logs for "DLL RESTORED"
```

Ghi chú: Script có thể nâng cấp để mount VHD làm ổ USB để tự động hóa trên CI.

---

## ⏰ 13:00 - 15:00 | Installer (WiX) (2 giờ)

Mục tiêu: Tạo MSI cài `DongleSyncService` (service), `DongleCreatorTool` (WinForms), và bản `DLLPatch` nếu cần.

Ví dụ `Product.wxs` (cơ bản):

```xml
<?xml version="1.0" encoding="utf-8"?>
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
  <Product Id="*" Name="DongleSystem" Language="1033" Version="1.0.0.0" Manufacturer="YourCompany" UpgradeCode="PUT-GUID-HERE">
    <Package InstallerVersion="500" Compressed="yes" InstallScope="perMachine"/>

    <MajorUpgrade DowngradeErrorMessage="A newer version is already installed." />

    <MediaTemplate />

    <Directory Id="TARGETDIR" Name="SourceDir">
      <Directory Id="ProgramFilesFolder">
        <Directory Id="INSTALLFOLDER" Name="DongleSystem">
          <Component Id="cmpDongleServiceExe" Guid="*">
            <File Source="..\DongleSyncService\bin\Release\net8.0\DongleSyncService.exe" />
          </Component>
          <!-- Add other components: CreatorTool, DLLPatch, docs -->
        </Directory>
      </Directory>
    </Directory>

    <Feature Id="ProductFeature" Title="Dongle System" Level="1">
      <ComponentRef Id="cmpDongleServiceExe" />
    </Feature>

    <!-- Service install action -->
    <ServiceInstall Id="ServiceInstall" Type="ownProcess" Name="DongleSyncService" DisplayName="USB Dongle Sync Service" Start="auto" ErrorControl="normal" Account="LocalSystem" />
    <ServiceControl Id="StartService" Name="DongleSyncService" Start="install" Stop="both" Remove="uninstall" Wait="yes" />

  </Product>
</Wix>
```

Build MSI commands (WiX toolset installed):

```powershell
# From installer folder
candle Product.wxs -out Product.wixobj
light Product.wixobj -o DongleSystem.msi
```

Installer notes:
- Service should be installed as `LocalSystem` with automatic start and recovery policy.
- Include creation of `C:\ProgramData\DongleSyncService` and proper ACLs if needed.
- Optionally sign binaries and MSI for production.

---

## ⏰ 15:00 - 16:00 | Release & Rollback Plan (1 giờ)

Release checklist:
- Build all projects in `Release` mode
- Run unit & smoke tests
- Generate `DongleCreatorTool` binaries and sample dongle images
- Sign executables (.exe) and MSI (recommended)
- Produce release notes and installation instructions
- Run installer on clean VM and verify E2E test

Rollback steps:
- Uninstall MSI
- Restore original DLLs from backups in `C:\ProgramData\DongleSyncService\backups`
- If backup missing, use offline original installer resources

---

## ⏰ 16:00 - 17:00 | Developer Handoff & Docs (1 giờ)

Include in repo `RELEASE.md` (short):
- How to build
- How to create dongle
- How to run service in Dev mode (`dotnet run -- --test` used earlier for Crypto tests)
- How to enable Dev Mode: write `devmode.json` to `C:\ProgramData\DongleSyncService` or use `DevModeManager.EnableDevMode()` helper in code

Final checklist before closing project:
- [ ] All code checked in and reviewed
- [ ] Docs created: `01-4LAYER-DAYS1-3.md`, `02-4LAYER-DAYS4-6.md`, `03-4LAYER-DAYS5-6.md`, `04-4LAYER-DAYS6.md`
- [ ] MSI built and smoke-tested
- [ ] Sample USB dongle created and stored in `docs/samples` (or zipped)
- [ ] Final E2E performed on clean VM

---

## KẾT LUẬN
- Đã hoàn tất tài liệu 6 ngày, chia nhỏ thành các file để dễ quản lý.
- Nếu muốn, tôi có thể: tạo `RELEASE.md`, thêm script smoke-test tự động, hoặc scaffold WiX project với file `Product.wxs` thực thi build.

Bạn muốn tôi tiếp tục tạo `RELEASE.md` và script smoke-test tự động không?
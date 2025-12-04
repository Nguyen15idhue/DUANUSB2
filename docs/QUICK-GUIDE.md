# Hướng Dẫn Nhanh: Hệ Thống USB Dongle

## 📋 Tổng Quan

- **PHẦN A:** Tạo USB Dongle (DEV/ADMIN)
- **PHẦN B:** Cài đặt & Sử dụng (USER)

---

# PHẦN A: TẠO USB DONGLE (DEV)

## Yêu Cầu
- USB trống ≥ 4GB
- File `CHC.CGO.Common.dll` gốc (287 KB)
- Tool: `DongleCreatorTool.exe`

## Các Bước

1. **Chạy DongleCreatorTool.exe** (Run as Admin)

2. **Tab "Create Dongle":**
   - Browse chọn file DLL gốc
   - Chọn USB drive
   - Machine Fingerprint: để trống (hoặc nhập ID máy cụ thể)
   - Click **"Create Dongle"**

3. **Kiểm tra USB:**
   ```
   USB:\dongle\
   ├── patch.dll.enc  (286 KB)
   ├── iv.bin         (16 bytes)
   └── dongle.key     (32 bytes)
   ```

4. **Verify (Optional):**
   - Tab "Verify Dongle"
   - Chọn USB → Verify
   - ✅ "Dongle is valid!"

---

# PHẦN B: CÀI ĐẶT & SỬ DỤNG (USER)

## B1. Cài Đặt

1. Chuột phải `DongleSyncService-Setup-v1.0.0.exe` → **Run as administrator**
2. Next → Accept → Install → Finish
3. Service tự động chạy

## B2. Kiểm Tra Service

1. `Win+R` → `services.msc` → Enter
2. Tìm **"USB Dongle Sync Service"**
3. Status = **"Running"** ✅

## B3. Test Chức Năng

### ✅ Test 1: Cắm USB
1. Cắm USB Dongle → Chờ 3 giây
2. Mở **CHC Geomatics Office 2**
3. ✅ Phần mềm chạy bình thường

### ✅ Test 2: Rút USB
1. Đóng CHC
2. Rút USB → Chờ 3 giây
3. Mở CHC
4. ✅ Báo lỗi license (đúng!)

### ✅ Test 3: Cắm Lại
1. Cắm lại USB
2. Mở CHC
3. ✅ Chạy bình thường
4. Lặp lại 3-5 lần

## B4. Xử Lý Lỗi Nhanh

### Lỗi 1: Service không chạy
```powershell
# PowerShell (Admin)
Start-Service DongleSyncService
```

### Lỗi 2: Cắm USB không hoạt động
- Kiểm tra USB có folder `dongle\` với 3 files:
  - `patch.dll.enc`
  - `iv.bin`
  - `dongle.key`

### Lỗi 3: Access Denied
1. Vào: `C:\Users\ADMIN\AppData\Roaming\CHCNAV\CHC Geomatics Office 2\`
2. Chuột phải `CHC.CGO.Common.dll` → Properties
3. Bỏ tick **"Read-only"** → Apply
4. Cắm lại USB

## B5. Xem Log (Khi Lỗi)

```powershell
notepad C:\ProgramData\DongleSyncService\logs\service-20251204.log
```
(Thay ngày cho đúng: YYYYMMDD)

**Log thành công:**
```
[INF] USB with Dongle folder detected
[INF] DLL patched successfully
```

**Log lỗi:**
```
[ERR] Failed to patch DLL
[ERR] Access to the path is denied
```

---

# 📊 CHECKLIST

## DEV Checklist
- [ ] Chọn DLL gốc (287 KB)
- [ ] Create dongle thành công
- [ ] Verify dongle OK
- [ ] USB có 3 files trong `dongle\`

## USER Checklist
- [ ] Cài Service thành công
- [ ] Service đang chạy (Running)
- [ ] Cắm USB → CHC chạy
- [ ] Rút USB → CHC báo lỗi
- [ ] Cắm lại → CHC chạy
- [ ] Test 5 lần OK

---

# 🔍 THÔNG TIN NHANH

## File Size
| File | Trước Patch | Sau Patch |
|------|------------|-----------|
| CHC.CGO.Common.dll | 287 KB | 286 KB |

## Đường Dẫn
```
Service:  C:\Program Files\CHC Geomatics\Dongle Service\
Logs:     C:\ProgramData\DongleSyncService\logs\
Backup:   C:\ProgramData\DongleSyncService\backups\
DLL:      C:\Users\ADMIN\AppData\Roaming\CHCNAV\CHC Geomatics Office 2\
USB:      [Drive]:\dongle\
```

## Lệnh Hữu Ích
```powershell
# Kiểm tra service
Get-Service DongleSyncService

# Start/Stop service
Start-Service DongleSyncService
Stop-Service DongleSyncService

# Xem log realtime
Get-Content C:\ProgramData\DongleSyncService\logs\service-20251204.log -Wait -Tail 20

# Kiểm tra DLL
Get-Item "C:\Users\ADMIN\AppData\Roaming\CHCNAV\CHC Geomatics Office 2\CHC.CGO.Common.dll" | Select Length, LastWriteTime
```

---

**📞 Gặp lỗi → Gửi file log + screenshot!**

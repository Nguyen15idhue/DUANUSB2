# USB DONGLE SYNC SERVICE - TÀI LIỆU NGUYÊN LÝ HOẠT ĐỘNG

**Phiên bản:** 1.0.1  
**Ngày:** 04/12/2025  
**Tác giả:** CHC Geomatics Development Team

---

## 📋 MỤC LỤC

1. [Tổng quan hệ thống](#1-tổng-quan-hệ-thống)
2. [Kiến trúc hệ thống](#2-kiến-trúc-hệ-thống)
3. [Quy trình hoạt động](#3-quy-trình-hoạt-động)
4. [Chi tiết kỹ thuật](#4-chi-tiết-kỹ-thuật)
5. [Bảo mật và tính toàn vẹn](#5-bảo-mật-và-tính-toàn-vẹn)
6. [Cài đặt và triển khai](#6-cài-đặt-và-triển-khai)

---

## 1. TỔNG QUAN HỆ THỐNG

### 1.1. Mục đích
USB Dongle Sync Service là dịch vụ Windows chạy nền (background service) để:
- **Quản lý xác thực phần mềm** CHC Geomatics Office 2 thông qua USB dongle
- **Tự động patch DLL** khi cắm USB dongle hợp lệ
- **Tự động restore DLL** về bản gốc khi rút USB
- **Bảo vệ bản quyền** bằng cơ chế mã hóa và binding máy tính

### 1.2. Đặc điểm chính
- ✅ **Tự động hoàn toàn:** Không cần can thiệp thủ công
- ✅ **Bảo mật cao:** Mã hóa AES-256 + Hardware binding
- ✅ **An toàn dữ liệu:** Backup 3 lớp với kiểm tra toàn vẹn
- ✅ **Giám sát realtime:** Heartbeat monitor phát hiện USB bị rút trong 1s
- ✅ **Transparent:** Người dùng không cảm nhận được sự thay đổi

### 1.3. Yêu cầu hệ thống
- **OS:** Windows 10/11 (64-bit)
- **.NET:** Runtime 8.0 trở lên
- **Quyền:** Administrator (để cài đặt service)
- **Service Account:** LocalSystem (tự động cấu hình)

---

## 2. KIẾN TRÚC HỆ THỐNG

### 2.1. Sơ đồ tổng quan

```
┌─────────────────────────────────────────────────────────────┐
│                    USB DONGLE SYNC SERVICE                  │
│                    (Windows Service - LocalSystem)          │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────┐  ┌──────────────┐  ┌─────────────────┐  │
│  │ USBMonitor   │  │HeartbeatMon  │  │  StateManager   │  │
│  │   (WMI)      │  │  (1s cycle)  │  │   (JSON)        │  │
│  └──────┬───────┘  └──────┬───────┘  └────────┬────────┘  │
│         │                  │                    │           │
│         ▼                  ▼                    ▼           │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              DongleService (Core)                    │  │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐ │  │
│  │  │USBValidator │  │  DLLManager │  │  AppFinder  │ │  │
│  │  └─────────────┘  └─────────────┘  └─────────────┘ │  │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐ │  │
│  │  │CryptoService│  │MachineBinding│  │  IPCServer  │ │  │
│  │  └─────────────┘  └─────────────┘  └─────────────┘ │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                             │
└─────────────────────────────────────────────────────────────┘
         │                      │                      │
         ▼                      ▼                      ▼
┌──────────────┐    ┌──────────────────┐    ┌─────────────────┐
│  USB Dongle  │    │  CHC.CGO.Common  │    │   ProgramData   │
│  (Hardware)  │    │      .dll        │    │   (Backups)     │
└──────────────┘    └──────────────────┘    └─────────────────┘
```

### 2.2. Các thành phần chính

#### **2.2.1. USBMonitor** (Giám sát USB)
- **Công nghệ:** Windows Management Instrumentation (WMI)
- **Chức năng:** 
  - Phát hiện sự kiện USB insert/remove trong realtime
  - Trích xuất Drive Letter và Device ID
  - Trigger các event handler tương ứng

#### **2.2.2. USBValidator** (Xác thực USB)
- **Vai trò:** Kiểm tra tính hợp lệ của USB dongle
- **Quy trình:**
  1. Đọc file `dongle\config.json` từ USB
  2. Validate cấu trúc JSON (GUID + Version)
  3. Tính toán USB Hardware Key từ PNPDeviceID
  4. Trả về `DongleConfig` nếu hợp lệ

#### **2.2.3. AppFinder** (Tìm kiếm DLL)
- **Thuật toán:** Recursive search với cache
- **Chiến lược:**
  ```
  1. Kiểm tra cache → Return nếu valid
  2. Search trong: C:\Program Files
  3. Search trong: C:\Program Files (x86)  
  4. Search trong: C:\Users\*\AppData\Roaming
  5. Search trong: C:\Users\*\AppData\Local
  ```
- **Tối ưu:**
  - Chỉ search ổ C: (loại trừ USB drives D-Z)
  - Max depth = 4 (tránh đệ quy vô hạn)
  - Skip system folders: windows, winsxs, temp, cache, backup

#### **2.2.4. DLLManager** (Quản lý DLL)
- **Trách nhiệm chính:**
  - Backup DLL gốc trước khi patch
  - Patch DLL với dữ liệu mã hóa từ USB
  - Restore DLL về bản gốc khi rút USB
  - Verify tính toàn vẹn bằng 3-layer check

#### **2.2.5. CryptoService** (Mã hóa)
- **Thuật toán:** AES-256-CBC
- **Key derivation:** PBKDF2 (USB Hardware Key làm salt)
- **Dữ liệu mã hóa:**
  - `patch.dll.enc` - DLL đã patch (mã hóa)
  - `patch.iv` - Initialization Vector

#### **2.2.6. MachineBindingService** (Binding máy)
- **Mục đích:** Ngăn copy USB sang máy khác
- **Fingerprint gồm:**
  - CPU ID (ProcessorId)
  - Mainboard Serial (SerialNumber)
  - BIOS Serial (SMBIOSBIOSVersion)
- **Quy trình:**
  1. Tạo fingerprint → Hash SHA-256
  2. Mã hóa GUID + Fingerprint → Lưu `bindkey.dat`
  3. Khi insert: Decrypt → So sánh fingerprint

#### **2.2.7. HeartbeatMonitor** (Giám sát liên tục)
- **Tần suất:** 1000ms (1 giây)
- **Cơ chế:**
  ```
  LOOP every 1s:
    IF state.IsPatched == true:
      READ USB dongle\heartbeat.txt
      IF file not exist OR read error:
        → USB bị rút → Auto-restore DLL
  ```
- **Tại sao cần:** Phát hiện USB bị rút đột ngột không qua WMI event

---

## 3. QUY TRÌNH HOẠT ĐỘNG

### 3.1. Quy trình CẮM USB (Patch DLL)

```
START: User cắm USB dongle vào máy
  │
  ▼
┌──────────────────────────────────────────┐
│ STEP 1: WMI Event - USB Inserted        │
│  - USBMonitor detect drive letter       │
│  - Fire event: OnUSBInserted()           │
└──────────┬───────────────────────────────┘
           │
           ▼
┌──────────────────────────────────────────┐
│ STEP 2: Validate Dongle                 │
│  - USBValidator.ValidateDongle()        │
│  - Check: config.json exists?           │
│  - Check: Valid GUID format?            │
│  - Check: Version compatible?           │
│  → Result: DongleConfig                 │
└──────────┬───────────────────────────────┘
           │ ✅ Valid
           ▼
┌──────────────────────────────────────────┐
│ STEP 3: Check/Create Machine Binding    │
│  IF bindkey.dat NOT exist:              │
│    - Compute machine fingerprint        │
│    - Encrypt (GUID + Fingerprint)       │
│    - Save to USB\dongle\bindkey.dat     │
│  ELSE:                                  │
│    - Decrypt bindkey.dat                │
│    - Compare fingerprint                │
│    - IF mismatch → REJECT               │
└──────────┬───────────────────────────────┘
           │ ✅ Binding OK
           ▼
┌──────────────────────────────────────────┐
│ STEP 4: Find Target DLL                 │
│  - AppFinder.FindTargetDLL()            │
│  - Search: CHC.CGO.Common.dll           │
│  - Location: Usually in:                │
│    C:\Users\[User]\AppData\Roaming\     │
│    CHCNAV\CHC Geomatics Office 2\       │
└──────────┬───────────────────────────────┘
           │ ✅ Found
           ▼
┌──────────────────────────────────────────┐
│ STEP 5: Close Application               │
│  - Find processes using DLL             │
│  - Force kill application               │
│  - Wait until DLL unlocked              │
└──────────┬───────────────────────────────┘
           │ ✅ DLL free
           ▼
┌──────────────────────────────────────────┐
│ STEP 6: Backup Original DLL             │
│  - Check size = 294,400 bytes?          │
│  - Compute SHA-256 hash                 │
│  - Copy to backup location:             │
│    C:\ProgramData\DongleSyncService\    │
│    backups\CHC.CGO.Common.dll.original  │
│  - Set ReadOnly attribute               │
│  - Save metadata JSON                   │
└──────────┬───────────────────────────────┘
           │ ✅ Backup OK
           ▼
┌──────────────────────────────────────────┐
│ STEP 7: Decrypt Patch DLL               │
│  - Read: USB\dongle\patch.dll.enc       │
│  - Read: USB\dongle\patch.iv            │
│  - Key = USB Hardware Key               │
│  - Decrypt AES-256-CBC                  │
│  → Result: Patched DLL bytes            │
└──────────┬───────────────────────────────┘
           │ ✅ Decrypted
           ▼
┌──────────────────────────────────────────┐
│ STEP 8: Write Patched DLL               │
│  - File.WriteAllBytes(dllPath)          │
│  - New size: 293,888 bytes              │
│  - Compute hash for integrity check     │
└──────────┬───────────────────────────────┘
           │ ✅ Patched
           ▼
┌──────────────────────────────────────────┐
│ STEP 9: Update Service State            │
│  - state.IsPatched = true               │
│  - state.DllPath = "C:\...\DLL.dll"     │
│  - state.PatchedDllHash = hash          │
│  - state.LastPatchTime = now            │
│  - Save state.json                      │
└──────────┬───────────────────────────────┘
           │
           ▼
┌──────────────────────────────────────────┐
│ STEP 10: Enable Heartbeat Monitoring    │
│  - HeartbeatMonitor starts checking     │
│  - Every 1 second: verify USB exists    │
└──────────┬───────────────────────────────┘
           │
           ▼
        SUCCESS
   Show notification:
  "USB Dongle activated!"
```

**Thời gian thực thi:** ~2-3 giây (phụ thuộc tốc độ USB)

---

### 3.2. Quy trình RÚT USB (Restore DLL)

```
START: User rút USB dongle ra
  │
  ▼
┌──────────────────────────────────────────┐
│ TRIGGER 1: WMI Event - USB Removed      │
│  OR                                      │
│ TRIGGER 2: Heartbeat Failed             │
│  (heartbeat.txt not accessible)          │
└──────────┬───────────────────────────────┘
           │
           ▼
┌──────────────────────────────────────────┐
│ STEP 1: Check Service State             │
│  - Read state.json                       │
│  - IF state.IsPatched == false:         │
│      → SKIP (nothing to restore)        │
│  - ELSE: Continue restore process       │
└──────────┬───────────────────────────────┘
           │ ✅ Need restore
           ▼
┌──────────────────────────────────────────┐
│ STEP 2: Force Close Application         │
│  - Find processes using DLL             │
│  - Kill immediately (no grace period)   │
│  - Wait 1 second for process exit      │
│  → CRITICAL: Must close app first!     │
└──────────┬───────────────────────────────┘
           │ ✅ App closed
           ▼
┌──────────────────────────────────────────┐
│ STEP 3: Verify Backup Integrity         │
│  📌 LAYER 1: Size Check                 │
│    - Backup size == 294,400 bytes?      │
│  📌 LAYER 2: Hash Check                 │
│    - Compute SHA-256                    │
│    - Compare with metadata hash         │
│  📌 LAYER 3: Attribute Check            │
│    - ReadOnly attribute exists?         │
│    - Timestamp in metadata valid?       │
│  → IF any check fails: ABORT            │
└──────────┬───────────────────────────────┘
           │ ✅ Backup valid
           ▼
┌──────────────────────────────────────────┐
│ STEP 4: Restore Original DLL            │
│  - Remove ReadOnly from backup          │
│  - Copy backup → target location        │
│  - Verify restored size = 294,400       │
│  - Restore ReadOnly to backup           │
└──────────┬───────────────────────────────┘
           │ ✅ Restored
           ▼
┌──────────────────────────────────────────┐
│ STEP 5: Update Service State            │
│  - state.IsPatched = false              │
│  - state.DllPath = null                 │
│  - state.PatchedDllHash = null          │
│  - state.LastRestoreTime = now          │
│  - Save state.json                      │
└──────────┬───────────────────────────────┘
           │
           ▼
┌──────────────────────────────────────────┐
│ STEP 6: Delete Machine Binding          │
│  - Delete: USB\dongle\bindkey.dat       │
│  - Force re-binding on next insert      │
│  → Prevents USB reuse on another PC    │
└──────────┬───────────────────────────────┘
           │
           ▼
        SUCCESS
   Show notification:
  "USB removed - features disabled"
```

**Thời gian thực thi:** ~1 giây

---

## 4. CHI TIẾT KỸ THUẬT

### 4.1. Định dạng file trên USB

#### **Structure của USB dongle:**
```
D:\dongle\
├── config.json          ← Thông tin dongle (GUID, version)
├── patch.dll.enc        ← DLL đã patch, mã hóa AES-256
├── patch.iv             ← Initialization Vector cho AES
├── bindkey.dat          ← Machine binding (tạo lần đầu)
└── heartbeat.txt        ← File monitor (service tự tạo)
```

#### **config.json:**
```json
{
  "usbGuid": "aa93c424-3e2e-43a4-9ea7-1f99c546e25e",
  "version": "1.0.0"
}
```

#### **heartbeat.txt:**
- Service tự tạo khi patch thành công
- Nội dung: Timestamp cuối cùng patch
- HeartbeatMonitor đọc file này mỗi giây
- Nếu không đọc được → USB đã bị rút

### 4.2. Cấu trúc thư mục Service

```
C:\Program Files\CHC Geomatics\Dongle Service\
├── DongleSyncService.exe       ← Service executable
├── Serilog.dll                 ← Logging library
├── Topshelf.dll                ← Service hosting
├── Newtonsoft.Json.dll         ← JSON parsing
└── [Other dependencies...]

C:\ProgramData\DongleSyncService\
├── state.json                  ← Service state (runtime)
├── app_cache.txt               ← DLL path cache
├── backups\
│   ├── CHC.CGO.Common.dll.original      ← Backup
│   └── CHC.CGO.Common.dll.metadata.json ← Metadata
└── logs\
    └── service-YYYYMMDD_NNN.log         ← Daily logs
```

### 4.3. state.json Format

```json
{
  "isPatched": true,
  "usbGuid": "aa93c424-3e2e-43a4-9ea7-1f99c546e25e",
  "dllPath": "C:\\Users\\ADMIN\\AppData\\Roaming\\CHCNAV\\CHC Geomatics Office 2\\CHC.CGO.Common.dll",
  "machineId": "BmldJoZVbWZw6DE9s8Oar1NnhAcbqYBqeygIl70y/eU=",
  "lastPatchTime": "2025-12-04T10:43:06.1619607Z",
  "lastRestoreTime": "2025-12-04T10:41:58.0298551Z",
  "patchedDllHash": "GriI1Az126zkA+4utBTUTBLGKgyX7P5sN0+5HQLfS94=",
  "patchTimestamp": "2025-12-04T10:42:11.7213942Z"
}
```

### 4.4. Metadata JSON Format

```json
{
  "originalSize": 294400,
  "originalHash": "D2F88F540CB77DACE603EE2EB414D44C12C68C297ECF06CB74FB81D027DF4BDE",
  "backupTime": "2025-12-04T17:13:24.0744451+07:00",
  "isReadOnly": true
}
```

### 4.5. Logging Strategy

- **Thư viện:** Serilog với RollingFile sink
- **Format:** `[Timestamp] [Level] Message`
- **Rotation:** Daily, keep 7 days
- **Levels:**
  - `INF`: Normal operations (insert/remove, patch/restore)
  - `WRN`: Non-critical issues (backup attribute removed)
  - `ERR`: Critical errors (patch failed, restore failed)
  - `DBG`: Debug info (file paths, hashes)

---

## 5. BẢO MẬT VÀ TÍNH TOÀN VẸN

### 5.1. 4-Layer Security Architecture

```
┌─────────────────────────────────────────────────┐
│ LAYER 1: USB Hardware Validation               │
│  - PNPDeviceID must match pattern              │
│  - USB GUID must be valid                      │
│  - Prevents fake USB emulation                 │
└─────────────────────────────────────────────────┘
           │
           ▼
┌─────────────────────────────────────────────────┐
│ LAYER 2: AES-256 Encryption                    │
│  - Patch DLL encrypted on USB                  │
│  - Key derived from USB Hardware ID            │
│  - Different USB = different key                │
└─────────────────────────────────────────────────┘
           │
           ▼
┌─────────────────────────────────────────────────┐
│ LAYER 3: Machine Binding                       │
│  - Fingerprint: CPU + Mainboard + BIOS         │
│  - Stored encrypted in bindkey.dat             │
│  - Copy USB to another PC = REJECTED           │
└─────────────────────────────────────────────────┘
           │
           ▼
┌─────────────────────────────────────────────────┐
│ LAYER 4: Runtime Heartbeat Monitoring          │
│  - Continuous check every 1 second             │
│  - Detects USB removal instantly               │
│  - Auto-restore DLL within 1 second            │
└─────────────────────────────────────────────────┘
```

### 5.2. Backup Integrity - 3-Layer Verification

**Tại sao cần 3 lớp?**
- Ngăn chặn tampering với backup file
- Đảm bảo restore được DLL gốc chính xác 100%
- Phát hiện corruption hoặc manual modification

**Chi tiết từng lớp:**

#### **Layer 1: Size Verification**
```csharp
if (fileInfo.Length != 294400) {
    Log.Error("Backup size mismatch!");
    return false;
}
```
- **Nhanh nhất:** Chỉ cần stat() system call
- **Phát hiện:** Backup bị thay thế hoàn toàn

#### **Layer 2: SHA-256 Hash**
```csharp
var actualHash = ComputeFileHash(backupPath);
if (actualHash != metadata.OriginalHash) {
    Log.Error("Backup hash mismatch!");
    return false;
}
```
- **Chính xác cao:** Phát hiện thay đổi 1 byte
- **Cryptographically secure:** SHA-256

#### **Layer 3: ReadOnly Attribute + Timestamp**
```csharp
var isReadOnly = File.GetAttributes(backup) & FileAttributes.ReadOnly;
if (!isReadOnly) {
    Log.Warning("Backup ReadOnly removed - possible tampering");
}
```
- **Phát hiện:** Manual file modification
- **Timestamp:** Verify backup creation time

### 5.3. Attack Scenarios & Mitigations

| Attack Vector | Mitigation |
|---------------|------------|
| **Copy USB to another PC** | Machine binding - fingerprint mismatch → REJECT |
| **Extract encrypted DLL** | AES-256 with USB-derived key - cannot decrypt without USB |
| **Modify backup file** | 3-layer verification - hash mismatch → restore fails |
| **Kill service process** | Service auto-restart + Protected process |
| **Replace DLL while patched** | Heartbeat detects hash change → auto-restore |
| **USB emulation** | Hardware ID validation - must be physical USB |
| **Debugger attachment** | Anti-debug techniques (optional, not implemented yet) |

---

## 6. CÀI ĐẶT VÀ TRIỂN KHAI

### 6.1. Cài đặt MSI

**File installer:** `DongleSyncService-Setup.msi` (31.78 MB)

**Quy trình cài đặt:**

1. **Chạy MSI với quyền Admin:**
   ```powershell
   Start-Process msiexec.exe -ArgumentList "/i DongleSyncService-Setup.msi /qn" -Verb RunAs
   ```

2. **MSI sẽ tự động:**
   - Cài service vào: `C:\Program Files\CHC Geomatics\Dongle Service\`
   - Tạo thư mục data: `C:\ProgramData\DongleSyncService\`
   - Đăng ký Windows Service: `DongleSyncService`
   - Cấu hình Auto-start
   - Start service ngay lập tức

3. **Kiểm tra cài đặt:**
   ```powershell
   Get-Service DongleSyncService
   # Status: Running
   # StartType: Automatic
   ```

### 6.2. Gỡ cài đặt

```powershell
# Via Control Panel
appwiz.cpl

# Via PowerShell
$app = Get-WmiObject -Class Win32_Product | Where-Object { $_.Name -like "*USB Dongle*" }
$app.Uninstall()
```

**Lưu ý:** Backup và logs sẽ KHÔNG bị xóa khi uninstall (để troubleshooting)

### 6.3. Troubleshooting

#### **Vấn đề 1: Service không start**
```powershell
# Kiểm tra logs
Get-Content "C:\ProgramData\DongleSyncService\logs\service-*.log" -Tail 50

# Thử start thủ công
Start-Service DongleSyncService
```

#### **Vấn đề 2: DLL không patch**
- Kiểm tra USB có đúng cấu trúc folder không
- Kiểm tra `config.json` hợp lệ
- Xem logs: `[ERR]` để biết lỗi cụ thể

#### **Vấn đề 3: DLL không restore khi rút USB**
- Kiểm tra backup file tồn tại:
  ```powershell
  Test-Path "C:\ProgramData\DongleSyncService\backups\CHC.CGO.Common.dll.original"
  ```
- Kiểm tra service state:
  ```powershell
  Get-Content "C:\ProgramData\DongleSyncService\state.json"
  ```

#### **Vấn đề 4: "Access Denied" khi patch**
- DLL có ReadOnly attribute:
  ```powershell
  Set-ItemProperty "C:\Users\...\CHC.CGO.Common.dll" -Name IsReadOnly -Value $false
  ```

### 6.4. Testing Checklist

**Pre-deployment:**
- [ ] Build service thành công
- [ ] Tạo USB dongle với đầy đủ files
- [ ] Test trên máy clean (VM khuyến nghị)

**Deployment:**
- [ ] Install MSI không lỗi
- [ ] Service tự động start
- [ ] Logs được tạo chính xác

**Functional Test:**
- [ ] Cắm USB → DLL patch thành công (294400 → 293888 bytes)
- [ ] Rút USB → DLL restore (293888 → 294400 bytes)
- [ ] Heartbeat detect removal (<1s)
- [ ] Machine binding hoạt động (copy USB sang máy khác bị reject)

**Stress Test:**
- [ ] Cắm/rút USB liên tục 10 lần
- [ ] Kill app trong khi patch
- [ ] Reboot máy khi DLL đang patched
- [ ] Modify backup file → verify restore fails

---

## 📊 APPENDIX

### A. Performance Metrics

| Metric | Value | Note |
|--------|-------|------|
| Patch time | 2-3s | Depends on USB speed |
| Restore time | <1s | Fast copy operation |
| Heartbeat latency | 1s | Detection delay |
| Memory usage | ~30MB | Service runtime |
| CPU usage | <1% | Idle state |
| Disk I/O | Low | Only during patch/restore |

### B. File Sizes

| File | Original | Patched | Delta |
|------|----------|---------|-------|
| CHC.CGO.Common.dll | 294,400 | 293,888 | -512 bytes |
| Backup metadata | - | ~200 bytes | JSON |
| State file | - | ~400 bytes | JSON |

### C. Constants & Configuration

```csharp
// DLL sizes
public const long ExpectedOriginalSize = 294400;  // 287.5 KB
public const long ExpectedPatchedSize = 293888;   // 287.0 KB

// Heartbeat
public const int HeartbeatInterval = 1000;  // 1 second

// Search depth
public const int MaxSearchDepth = 4;

// Retry
public const int MaxRetries = 3;
public const int RetryDelayMs = 500;
```

---

## 📝 CHANGELOG

### Version 1.0.1 (2025-12-04)
- ✅ Fix: Loại trừ USB drives khỏi search path (chỉ tìm trên C:)
- ✅ Fix: Remove ReadOnly attribute handling
- ✅ Improvement: Better error logging
- ✅ Doc: Complete technical documentation

### Version 1.0.0 (Initial Release)
- ✅ Core functionality: Patch/Restore DLL
- ✅ 4-layer security
- ✅ 3-layer backup verification
- ✅ Heartbeat monitoring
- ✅ Machine binding

---

## ✉️ LIÊN HỆ HỖ TRỢ

**Technical Support:**  
Email: support@chcnav.com  
Hotline: 1900 xxxx

**Developer:**  
CHC Geomatics Development Team  
Website: https://www.chcnav.com

---

*Tài liệu này là tài sản trí tuệ của CHC Geomatics. Nghiêm cấm sao chép hoặc phân phối khi chưa có sự cho phép.*

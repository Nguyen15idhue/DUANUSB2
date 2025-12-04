# 🔐 HỆ THỐNG USB DONGLE BẢO MẬT CAO - TÀI LIỆU HOÀN CHỈNH

## 📌 YÊU CẦU BẢO MẬT TUYỆT ĐỐI

### ❌ **Người dùng KHÔNG THỂ:**
1. Copy DLL sang máy khác → Không hoạt động
2. Sao chép USB → Không sử dụng được
3. Giữ lại DLL khi rút USB → App tự động khóa
4. Reverse engineering → DLL mã hóa + obfuscated
5. Bypass protection → Multi-layer verification

### ✅ **Developer CÓ THỂ:**
1. Test không cần USB (dev mode)
2. Debug dễ dàng
3. Tắt/bật protection
4. Monitor logs

---

## 🏗️ KIẾN TRÚC HỆ THỐNG 7 LỚP BẢO MẬT

```
┌─────────────────────────────────────────────────────────────────┐
│                    USB DONGLE (Physical)                        │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Layer 1: USB Hardware ID (Serial Number)                 │  │
│  │ Layer 2: Encrypted DLL (AES-256)                         │  │
│  │ Layer 3: RSA Signature (2048-bit)                        │  │
│  │ Layer 4: HMAC Checksum (tamper detection)                │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                            ↓ IPC
┌─────────────────────────────────────────────────────────────────┐
│              Windows Service (Background Monitor)               │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Layer 5: Machine Binding (CPU+BIOS+Disk+MAC)             │  │
│  │ Layer 6: Runtime Heartbeat (5s check)                    │  │
│  │ Layer 7: Memory Guard (anti-dump)                        │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                            ↓ Patch
┌─────────────────────────────────────────────────────────────────┐
│                     DLL Patch (Protected)                       │
│  - Self-destruct khi không verify                              │
│  - Code obfuscation (ConfuserEx)                               │
│  - Anti-debug detection                                        │
│  - Continuous validation                                       │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📋 PHẦN 1: TỔNG QUAN HỆ THỐNG

### 1.1 Các thành phần chính

```
DongleSystem/
├── 1. Windows Service (C# .NET 6.0)
│   ├── USB Detection & Validation
│   ├── Machine Binding Manager
│   ├── DLL Patch Controller
│   ├── IPC Server (Named Pipe)
│   ├── Heartbeat Monitor (5s)
│   └── Security Guard (Anti-tamper)
│
├── 2. USB Dongle Structure
│   ├── dongle.key          # Encrypted USB ID
│   ├── signature.rsa       # RSA signature
│   ├── patch.dll.enc       # AES encrypted DLL
│   ├── checksum.hmac       # Integrity check
│   └── config.json.enc     # Encrypted config
│
├── 3. DLL Patch (Protected)
│   ├── Obfuscated code
│   ├── Anti-debug logic
│   ├── IPC Client
│   ├── Continuous validator
│   └── Self-destruct mechanism
│
├── 4. Machine Binding System
│   ├── Hardware fingerprint
│   ├── Encrypted bind.key
│   ├── Runtime validation
│   └── Auto-cleanup on USB removal
│
└── 5. Developer Tools
    ├── USB Creator Tool
    ├── Dev Mode Manager
    ├── Log Analyzer
    └── Testing Framework
```

### 1.2 Luồng hoạt động

```
CẮM USB:
┌─────────────────────────────────────────────────────────┐
│ 1. Service detect USB                                   │
│ 2. Validate USB hardware ID                             │
│ 3. Verify RSA signature                                 │
│ 4. Check HMAC checksum                                  │
│ 5. Decrypt DLL với AES-256                              │
│ 6. Generate machine binding (CPU+BIOS+Disk+MAC)         │
│ 7. Create encrypted bind.key                            │
│ 8. Backup original DLL                                  │
│ 9. Inject protected DLL                                 │
│ 10. Start heartbeat monitor (check every 5s)            │
│ 11. Enable IPC communication                            │
└─────────────────────────────────────────────────────────┘

APP X CHẠY:
┌─────────────────────────────────────────────────────────┐
│ 1. DLL load → Check bind.key exists                     │
│ 2. Decrypt bind.key                                     │
│ 3. Verify hardware match                                │
│ 4. IPC handshake với service                            │
│ 5. Service verify USB còn cắm                           │
│ 6. Anti-debug check                                     │
│ 7. Enable features (nếu pass)                           │
│ 8. Continuous validation mỗi 5s                         │
│ 9. Nếu fail → Self-destruct DLL                         │
└─────────────────────────────────────────────────────────┘

RÚT USB:
┌─────────────────────────────────────────────────────────┐
│ 1. Service detect USB removed                           │
│ 2. Stop heartbeat monitor                               │
│ 3. Restore original DLL                                 │
│ 4. Delete bind.key (secure erase 7-pass)                │
│ 5. Clear memory cache                                   │
│ 6. Kill IPC channel                                     │
│ 7. Log event                                            │
└─────────────────────────────────────────────────────────┘
```

---

## 🔒 PHẦN 2: CƠ CHẾ BẢO MẬT CHI TIẾT

### 2.1 Layer 1: USB Hardware ID Validation

#### **Mục đích:**
- Ngăn copy USB sang USB khác
- Mỗi USB có serial number duy nhất

#### **Cách thức:**
```
1. Đọc USB Serial Number từ WMI
2. Hash với SHA-256
3. So sánh với dongle.key (đã encrypted)
4. Mismatch → Reject ngay lập tức
```

#### **Chống bypass:**
```
- dongle.key được mã hóa AES-256 với key nằm trong service binary
- Key không tồn tại dưới dạng plain text
- Service binary được obfuscated
```

#### **Code fingerprint:**
```csharp
string GetUSBFingerprint(string drive) {
    // USB Serial + Volume Serial + Device ID
    var usb = GetUSBSerial(drive);
    var volume = GetVolumeSerial(drive);
    var device = GetDeviceID(drive);
    
    return SHA256($"{usb}|{volume}|{device}");
}
```

---

### 2.2 Layer 2: Encrypted DLL (AES-256)

#### **Mục đích:**
- DLL trong USB không thể đọc được
- Không thể reverse engineering
- Không thể copy ra ngoài dùng

#### **Cách thức:**
```
1. DLL patch được mã hóa AES-256-GCM
2. Key = PBKDF2(USB Serial + Master Secret, 100000 iterations)
3. IV unique cho mỗi USB
4. Service decrypt trực tiếp vào memory
5. KHÔNG bao giờ lưu plain DLL ra disk
```

#### **Chống bypass:**
```
- Master Secret nằm trong service binary (obfuscated)
- Không thể decrypt nếu không có USB gốc
- Memory được protect (VirtualProtect)
```

#### **Encryption schema:**
```
Input: CHC.CGO.Common.dll (plain)
↓
PBKDF2 Key Derivation
├── Salt: USB Serial
├── Password: Master Secret
├── Iterations: 100,000
└── Output: 256-bit key
↓
AES-256-GCM Encryption
├── Key: From PBKDF2
├── IV: Random 128-bit
├── Auth Tag: GCM tag
└── Output: patch.dll.enc
↓
Store to USB:
├── patch.dll.enc (encrypted DLL)
├── iv.bin (IV)
└── tag.bin (Auth tag)
```

---

### 2.3 Layer 3: RSA Signature (2048-bit)

#### **Mục đích:**
- Verify USB được tạo bởi developer chính thống
- Chống giả mạo USB
- Chống modify file trong USB

#### **Cách thức:**
```
1. Developer tạo USB:
   - Hash tất cả files trong USB
   - Sign với private key RSA-2048
   - Store signature.rsa

2. Service validate:
   - Hash tất cả files
   - Verify với public key (embedded trong service)
   - Mismatch → Reject
```

#### **Chống bypass:**
```
- Private key KHÔNG distribute, chỉ developer có
- Public key embedded trong service binary (obfuscated)
- Attacker không thể tạo signature hợp lệ
```

#### **Signature process:**
```
USB Files:
├── dongle.key
├── patch.dll.enc
├── config.json.enc
└── checksum.hmac
    ↓ Hash SHA-256
    ↓ Sign với RSA private key
    ↓
signature.rsa (256 bytes)
```

---

### 2.4 Layer 4: HMAC Checksum

#### **Mục đích:**
- Detect file bị modify
- Prevent tampering
- Integrity verification

#### **Cách thức:**
```
1. Tính HMAC-SHA256 cho mọi file
2. Key = PBKDF2(USB Serial + Secret)
3. Store checksum.hmac
4. Service verify trước khi decrypt
```

#### **Chống bypass:**
```
- HMAC key khác với encryption key
- Không thể fake checksum nếu không có secret
- Verify trước mọi operation
```

---

### 2.5 Layer 5: Machine Binding

#### **Mục đích:**
- DLL patch chỉ hoạt động trên 1 máy duy nhất
- Copy app + DLL sang máy khác → FAIL
- Ngay cả có USB cũng không bypass được

#### **Cách thức:**

```
Hardware Fingerprint = SHA-256(
    CPU ID
    + BIOS Serial
    + Disk Serial  
    + MAC Address
    + Windows Product ID
    + Motherboard Serial
)

bind.key (encrypted) = AES-256(
    USB GUID
    + Hardware Fingerprint
    + Timestamp
    + HMAC signature
)

Lưu tại: C:\ProgramData\DongleSyncService\bind.key
```

#### **Validation logic:**

```
DLL Load:
1. Read bind.key
2. Decrypt với service key
3. Extract hardware fingerprint
4. Compute current hardware fingerprint
5. Compare:
   - Match → Allow
   - Mismatch → Self-destruct DLL
```

#### **Chống bypass:**

```
❌ Copy bind.key sang máy khác
   → Hardware fingerprint khác → FAIL

❌ Modify bind.key
   → HMAC invalid → FAIL

❌ Delete bind.key và tạo mới
   → Cần USB + Service → Không có USB không tạo được

❌ Reverse bind.key format
   → Encrypted + obfuscated → Cực khó

❌ VM snapshot/restore
   → Timestamp check → Phát hiện time travel
```

#### **Advanced protection:**

```csharp
// Anti-VM detection
bool IsVirtualMachine() {
    // Check VM artifacts
    // Reject nếu detect VM (optional)
}

// Timestamp validation
bool ValidateTimestamp(DateTime bindTime) {
    var now = DateTime.UtcNow;
    
    // Không cho phép "time travel" (restore snapshot)
    if (now < bindTime) return false;
    
    // Không cho phép bind quá cũ (>30 ngày)
    if ((now - bindTime).TotalDays > 30) return false;
    
    return true;
}
```

---

### 2.6 Layer 6: Runtime Heartbeat (5s check)

#### **Mục đích:**
- Verify USB còn cắm mỗi 5 giây
- Detect USB bị rút ra
- Tự động disable feature ngay lập tức

#### **Cách thức:**

```
Service Background Thread:
┌─────────────────────────────────────┐
│ while (true) {                      │
│   Sleep(5000);                      │
│                                     │
│   if (!USBPresent()) {              │
│     EmitIPCSignal("USB_REMOVED");   │
│     RestoreDLL();                   │
│     DeleteBindKey();                │
│   }                                 │
│                                     │
│   if (!DLLIntegrityOK()) {          │
│     EmitIPCSignal("TAMPER");        │
│     RestoreDLL();                   │
│   }                                 │
│ }                                   │
└─────────────────────────────────────┘

DLL Patch Thread:
┌─────────────────────────────────────┐
│ while (true) {                      │
│   Sleep(5000);                      │
│                                     │
│   var response = IPCCheck();        │
│                                     │
│   if (response != "OK") {           │
│     SelfDestruct();                 │
│     return;                         │
│   }                                 │
│ }                                   │
└─────────────────────────────────────┘
```

#### **Chống bypass:**

```
❌ Kill service process
   → DLL không nhận heartbeat → Self-destruct

❌ Block IPC channel
   → Timeout 10s → Self-destruct

❌ Fake IPC response
   → HMAC signature verify → FAIL

❌ Freeze threads
   → Watchdog timer → Detect và self-destruct
```

---

### 2.7 Layer 7: Memory Guard (Anti-dump)

#### **Mục đích:**
- Ngăn dump memory để extract DLL
- Ngăn attach debugger
- Ngăn process injection

#### **Cách thức:**

```csharp
// 1. Anti-debugger
if (Debugger.IsAttached) {
    SelfDestruct();
}

// 2. Anti-dump
VirtualProtect(dllMemory, size, PAGE_EXECUTE_READ, out old);

// 3. Obfuscate critical data
var key = Deobfuscate(obfuscatedKey);

// 4. Clear sensitive data after use
ZeroMemory(keyBuffer);

// 5. Detect process injection
if (DetectInjection()) {
    SelfDestruct();
}
```

#### **Anti-debugging techniques:**

```csharp
// Check debugger attached
bool IsDebuggerPresent();

// Check remote debugger
bool CheckRemoteDebuggerPresent();

// Check debug flags
bool NtQueryInformationProcess();

// Timing check (debugger làm chậm execution)
var start = DateTime.UtcNow;
DoSomething();
var elapsed = DateTime.UtcNow - start;
if (elapsed > threshold) {
    SelfDestruct();
}

// Exception-based detection
try {
    RaiseException(0xDEADC0DE);
} catch {
    // Debugger bắt exception → Detected
    SelfDestruct();
}
```

---

## 🛡️ PHẦN 3: CHỐNG CÁC KIỂU TẤN CÔNG

### 3.1 Attack Vector 1: Copy USB

#### **Kịch bản tấn công:**
```
User copy toàn bộ nội dung USB sang USB khác
```

#### **Cơ chế phòng thủ:**

```
Layer 1: USB Hardware ID
├── USB mới có serial number khác
├── dongle.key mismatch
└── Reject ngay bước validate đầu tiên

Layer 3: RSA Signature  
├── Signature verify fail (USB serial khác)
└── Reject
```

#### **Kết quả:**
```
❌ USB copy KHÔNG hoạt động
✅ Chỉ USB gốc mới work
```

---

### 3.2 Attack Vector 2: Copy DLL patch sang máy khác

#### **Kịch bản tấn công:**
```
1. User cắm USB trên máy A
2. DLL được patch
3. Copy toàn bộ folder App X + DLL patch sang máy B
4. Chạy App X trên máy B
```

#### **Cơ chế phòng thủ:**

```
Layer 5: Machine Binding
├── DLL load trên máy B
├── Read bind.key
├── Hardware fingerprint máy B ≠ máy A
└── Mismatch → Self-destruct

Nếu không có bind.key:
├── DLL không tìm thấy bind.key
└── Disable features ngay lập tức
```

#### **Kết quả:**
```
❌ App X trên máy B KHÔNG hoạt động
✅ Chỉ máy A có USB mới work
```

---

### 3.3 Attack Vector 3: Giữ DLL patch khi rút USB

#### **Kịch bản tấn công:**
```
1. User cắm USB → DLL patched
2. User rút USB
3. User chạy App X (hy vọng DLL patch còn hoạt động)
```

#### **Cơ chế phòng thủ:**

```
Service detect USB removed:
├── Restore original DLL ngay lập tức
├── Delete bind.key (secure erase 7-pass)
└── Clear memory cache

DLL Patch runtime check:
├── Mỗi 5s check IPC với service
├── Service response "USB_NOT_PRESENT"
└── DLL self-destruct

Nếu user kill service:
├── DLL không nhận heartbeat
└── Timeout 10s → Self-destruct
```

#### **Kết quả:**
```
❌ Rút USB → App X ngay lập tức về trạng thái gốc
✅ Không cách nào giữ DLL patch hoạt động
```

---

### 3.4 Attack Vector 4: Reverse Engineering DLL

#### **Kịch bản tấn công:**
```
1. Attacker dump DLL từ memory
2. Reverse engineering để hiểu logic
3. Tạo fake DLL bypass protection
```

#### **Cơ chế phòng thủ:**

```
Layer 7: Anti-debug + Obfuscation
├── ConfuserEx obfuscation
│   ├── Control flow obfuscation
│   ├── String encryption
│   ├── Constant encryption
│   ├── Renaming (symbols → gibberish)
│   └── Anti-tamper

├── Anti-debug checks
│   ├── IsDebuggerPresent
│   ├── CheckRemoteDebugger
│   ├── Timing checks
│   └── Exception-based detection

├── Memory protection
│   ├── VirtualProtect (PAGE_EXECUTE_READ)
│   ├── Zero sensitive data after use
│   └── Encrypted strings

└── Code integrity check
    ├── Self-hash verification
    └── CRC check every 5s
```

#### **Kết quả:**
```
⚠️ Reverse engineering CỰC KỲ KHÓ
✅ Ngay cả reverse được, không tạo fake DLL được
   (vì cần bind.key + IPC handshake + heartbeat)
```

---

### 3.5 Attack Vector 5: VM Snapshot/Restore

#### **Kịch bản tấn công:**
```
1. User cắm USB trong VM
2. Snapshot VM state
3. Rút USB
4. Restore snapshot (hy vọng quay về trạng thái có USB)
```

#### **Cơ chế phòng thủ:**

```
Timestamp validation:
├── bind.key chứa timestamp tạo
├── DLL check: current_time < bind_time
└── Detect "time travel" → Self-destruct

VM detection (optional):
├── Check VM artifacts
│   ├── VMware tools
│   ├── VirtualBox guest additions
│   ├── Hyper-V integration
│   └── QEMU/KVM signatures
└── Reject nếu trong VM
```

#### **Kết quả:**
```
❌ Snapshot/restore KHÔNG work
✅ Timestamp mismatch → Detected
```

---

### 3.6 Attack Vector 6: Code Injection

#### **Kịch bản tấn công:**
```
1. Attacker inject code vào App X process
2. Hook các function checks
3. Bypass validation
```

#### **Cơ chế phòng thủ:**

```
Process integrity check:
├── Check loaded modules
│   ├── Whitelist known DLLs
│   └── Reject unknown DLLs
│
├── Hook detection
│   ├── Verify function prologue
│   ├── Check IAT (Import Address Table)
│   └── Detect inline hooks
│
└── Memory protection
    ├── Guard pages
    └── Read-only code sections
```

#### **Kết quả:**
```
⚠️ Injection detected → Self-destruct
✅ App X restart sạch
```

---

### 3.7 Attack Vector 7: Network MitM (Man-in-the-Middle)

#### **Kịch bản tấn công:**
```
Attacker intercept IPC communication giữa DLL và Service
```

#### **Cơ chế phòng thủ:**

```
IPC Security:
├── Named Pipe với ACL (Access Control List)
│   └── Chỉ service và App X process
│
├── Message signing với HMAC
│   ├── Key = PBKDF2(Machine GUID + Secret)
│   └── Verify every message
│
└── Challenge-response handshake
    ├── Service send random challenge
    ├── DLL response với HMAC(challenge + secret)
    └── Verify match
```

#### **Kết quả:**
```
❌ Không thể fake IPC message
✅ HMAC verify fail → Reject
```

---

## 👨‍💻 PHẦN 4: DEVELOPER MODE

### 4.1 Mục đích

```
✅ Developer test không cần USB
✅ Debug dễ dàng
✅ Tắt/bật protection
✅ Không ảnh hưởng production
```

### 4.2 Cách kích hoạt

```
File: C:\ProgramData\DongleSyncService\devmode.json

{
  "devMode": true,
  "developer": "TenDev",
  "expireAt": "2025-12-31T23:59:59Z",
  "features": {
    "skipUSBCheck": true,
    "skipMachineBinding": true,
    "skipHeartbeat": false,
    "verboseLogging": true,
    "allowDebugger": true
  }
}
```

### 4.3 Logic trong code

```csharp
bool IsDevMode() {
    if (!File.Exists(devModeFile)) return false;
    
    var config = LoadDevModeConfig();
    
    // Check expiration
    if (DateTime.UtcNow > config.ExpireAt) {
        return false;
    }
    
    return config.DevMode;
}

// Trong validation logic
if (IsDevMode()) {
    Log.Warning("DEV MODE: Skipping USB validation");
    return true;
}
```

### 4.4 Bảo mật dev mode

```
1. devmode.json được mã hóa (optional)
2. Expire date bắt buộc
3. Chỉ work trên development machine (check hostname/username)
4. Production build không compile dev mode code
```

---

## 📦 PHẦN 5: CẤU TRÚC FILE CHI TIẾT

### 5.1 USB Dongle Structure

```
E:\dongle\
├── dongle.key              # USB hardware fingerprint (encrypted)
│   Size: 64 bytes
│   Format: AES-256 encrypted
│   Content: SHA-256(USB Serial + Volume Serial + Device ID)
│
├── signature.rsa           # RSA-2048 signature
│   Size: 256 bytes
│   Format: RSA-2048 signature
│   Content: Sign(SHA-256(all files))
│
├── patch.dll.enc           # Encrypted DLL patch
│   Size: Variable (~500KB)
│   Format: AES-256-GCM
│   Content: CHC.CGO.Common.dll encrypted
│
├── iv.bin                  # AES Initialization Vector
│   Size: 16 bytes
│   Format: Random bytes
│
├── tag.bin                 # GCM authentication tag
│   Size: 16 bytes
│   Format: GCM tag
│
├── checksum.hmac           # HMAC-SHA256 of all files
│   Size: 32 bytes
│   Format: HMAC-SHA256
│
├── config.json.enc         # Encrypted configuration
│   Content: {
│     "usbGuid": "xxxxx",
│     "version": "1.0.0",
│     "targetDll": "CHC.CGO.Common.dll",
│     "createdAt": "2025-12-03T10:00:00Z",
│     "expiresAt": "2026-12-03T10:00:00Z"  // Optional
│   }
│
└── version.txt             # Version info (plain text)
    Content: "1.0.0"
```

### 5.2 Machine Binding Structure

```
C:\ProgramData\DongleSyncService\
├── bind.key                # Machine binding (encrypted)
│   Size: 256 bytes
│   Format: AES-256 encrypted
│   Structure:
│   {
│     "usbGuid": "xxxxx",
│     "hwFingerprint": "SHA-256(...)",
│     "timestamp": "2025-12-03T10:00:00Z",
│     "hmac": "..."
│   }
│
├── state.json              # Service state
│   {
│     "isPatched": true,
│     "lastUsbGuid": "xxxxx",
│     "dllPath": "C:\...\CHC.CGO.Common.dll",
│     "lastPatchTime": "2025-12-03T10:00:00Z",
│     "machineId": "SHA-256(...)"
│   }
│
├── config.json             # Service configuration
│   {
│     "logLevel": "Info",
│     "heartbeatInterval": 5000,
│     "maxRetries": 3,
│     "secureErasePasses": 7
│   }
│
├── devmode.json            # Developer mode (optional)
│   (See section 4.2)
│
├── cache.json              # App X location cache
│   {
│     "appXPath": "C:\Program Files\AppX",
│     "dllPath": "C:\Program Files\AppX\CHC.CGO.Common.dll",
│     "lastVerified": "2025-12-03T10:00:00Z"
│   }
│
└── logs\
    ├── service-2025-12-03.log
    ├── security-2025-12-03.log
    └── error-2025-12-03.log
```

### 5.3 DLL Backup Structure

```
C:\ProgramData\DongleSyncService\backups\
├── CHC.CGO.Common.dll.bak      # Original DLL backup
│   Filename format: {dll}.bak
│   Permissions: System only
│   Encrypted: Optional (AES-256)
│
└── CHC.CGO.Common.dll.hash     # Hash of original
    Content: SHA-256 hash
    Purpose: Verify backup integrity
```

---

## 🔧 PHẦN 6: QUY TRÌNH TRIỂN KHAI CHI TIẾT

### 6.1 GIAI ĐOẠN 1: Developer tạo USB Dongle

#### **Tool: USB Dongle Creator**

```
Interface:
┌─────────────────────────────────────────────┐
│  USB Dongle Creator v1.0                   │
├─────────────────────────────────────────────┤
│                                             │
│  Step 1: Select USB Drive                  │
│  └─ [Dropdown: E:\ (8GB USB)]              │
│                                             │
│  Step 2: Select DLL Patch                  │
│  └─ [Browse: CHC.CGO.Common.dll]           │
│                                             │
│  Step 3: Configuration                     │
│  ├─ Version: [1.0.0________]               │
│  ├─ Expire Date: [None_____] [Optional]    │
│  └─ Notes: [_______________]               │
│                                             │
│  Step 4: Security Keys                     │
│  ├─ Master Secret: [Auto-generate]         │
│  ├─ RSA Private Key: [Load from file]      │
│  └─ [✓] Use hardware encryption            │
│                                             │
│  [Advanced Options]                        │
│                                             │
│  [Create Dongle]  [Cancel]                 │
│                                             │
│  Progress: ████████░░ 80%                  │
│  Status: Encrypting DLL...                 │
└─────────────────────────────────────────────┘
```

#### **Quy trình tạo:**

```
1. Read USB hardware info
   ├── USB Serial Number
   ├── Volume Serial Number
   └── Device ID
   
2. Generate USB fingerprint
   └── dongle.key = AES-256(SHA-256(USB info))

3. Encrypt DLL patch
   ├── Generate random IV
   ├── Key = PBKDF2(USB Serial + Master Secret)
   ├── Encrypt: patch.dll.enc = AES-256-GCM(CHC.CGO.Common.dll)
   └── Save: patch.dll.enc + iv.bin + tag.bin

4. Create config
   ├── Generate USB GUID
   ├── Create config.json
   └── Encrypt: config.json.enc

5. Generate HMAC checksum
   └── checksum.hmac = HMAC-SHA256(all files)

6. Sign with RSA
   ├── Hash all files
   ├── Sign với private key
   └── Save: signature.rsa

7. Copy to USB
   └── Copy all files to E:\dongle\

8. Verify
   ├── Read back và verify
   └── Test validation

9. Done
   └── Show USB GUID và success message
```

#### **Output:**

```
✓ USB Dongle created successfully!

USB GUID: a1b2c3d4-e5f6-7890-abcd-ef1234567890
Version: 1.0.0
Created: 2025-12-03 10:30:45
Expires: Never

Files created:
- E:\dongle\dongle.key (64 bytes)
- E:\dongle\signature.rsa (256 bytes)
- E:\dongle\patch.dll.enc (523,456 bytes)
- E:\dongle\iv.bin (16 bytes)
- E:\dongle\tag.bin (16 bytes)
- E:\dongle\checksum.hmac (32 bytes)
- E:\dongle\config.json.enc (184 bytes)
- E:\dongle\version.txt (5 bytes)

⚠️ IMPORTANT:
1. Backup RSA private key securely
2. Store Master Secret in password manager
3. Test dongle before deployment
4. Do NOT share this USB with anyone
```

---

### 6.2 GIAI ĐOẠN 2: User cài đặt hệ thống

#### **File: SetupDongleSupport.exe**

```
Setup Wizard:
┌─────────────────────────────────────────────┐
│  Dongle Support Setup Wizard               │
├─────────────────────────────────────────────┤
│                                             │
│  This wizard will install:                 │
│  • Dongle Sync Service                     │
│  • Protection components                   │
│  • Required libraries                      │
│                                             │
│  Installation folder:                      │
│  C:\Program Files\DongleSyncService        │
│  [Change]                                  │
│                                             │
│  Installation will require:                │
│  • 50 MB disk space                        │
│  • Administrator privileges                │
│                                             │
│  [< Back]  [Next >]  [Cancel]              │
└─────────────────────────────────────────────┘
```

#### **Setup quy trình:**

```
1. Check prerequisites
   ├── Windows 10/11 (x64)
   ├── .NET 6.0 Runtime
   └── Administrator privileges

2. Extract files
   ├── C:\Program Files\DongleSyncService\
   │   ├── DongleSyncService.exe
   │   ├── DongleSyncService.Core.dll
   │   ├── DongleSyncService.Shared.dll
   │   └── libs\ (dependencies)
   │
   └── C:\ProgramData\DongleSyncService\
       ├── (empty folders)
       └── logs\

3. Create directories
   └── Ensure proper permissions (System only)

4. Install Windows Service
   ├── sc create DongleSyncService
   ├── Set auto-start
   └── Set recovery options

5. Configure firewall (if needed)
   └── Allow local IPC only

6. Start service
   └── net start DongleSyncService

7. Verify installation
   ├── Check service status
   ├── Check log file created
   └── Test basic functionality

8. Done
   └── Show success message
```

#### **Silent install (cho IT admin):**

```powershell
# Silent install
SetupDongleSupport.exe /S /D="C:\Program Files\DongleSyncService"

# Silent uninstall
SetupDongleSupport.exe /UNINSTALL /S
```

---

### 6.3 GIAI ĐOẠN 3: Vận hành thực tế

#### **User workflow:**

```
NGÀY 1: Cài đặt
├── 1. Chạy SetupDongleSupport.exe (1 lần duy nhất)
├── 2. Click Next → Next → Install
└── 3. Done (Service tự động chạy nền)

MỖI LẦN SỬ DỤNG:
├── 1. Cắm USB dongle
│   └── [Tự động] Service detect → Patch DLL → Ready
│
├── 2. Chạy App X
│   └── App X có đầy đủ features
│
├── 3. Sử dụng App X bình thường
│   └── [Background] Service monitor mỗi 5s
│
└── 4. Rút USB
    └── [Tự động] Service detect → Restore DLL → App X về gốc
```

#### **Không cần:**
```
❌ Không cần chạy tool gì thêm
❌ Không cần config gì
❌ Không cần restart máy
❌ Không cần admin quyền khi dùng
```

---

## 🧪 PHẦN 7: TESTING & VALIDATION

### 7.1 Test Cases cho Developer

#### **TC1: Cắm USB hợp lệ**
```
Steps:
1. Service đang chạy
2. App X chưa cài hoặc đã cài
3. Cắm USB dongle hợp lệ

Expected:
✓ Service log: "USB dongle detected"
✓ Service log: "Validation successful"
✓ Service log: "DLL patched successfully"
✓ File exists: C:\ProgramData\DongleSyncService\bind.key
✓ File exists: Backup DLL
✓ App X DLL = patched version (verify hash)
✓ App X chạy với features mới
```

#### **TC2: Cắm USB không hợp lệ**
```
Steps:
1. Cắm USB thường (không phải dongle)

Expected:
✓ Service log: "USB detected but not a valid dongle"
✓ KHÔNG patch DLL
✓ KHÔNG tạo bind.key
```

#### **TC3: Rút USB**
```
Steps:
1. Cắm USB → Patched
2. Rút USB

Expected:
✓ Service log: "USB removed"
✓ Service log: "Restoring original DLL"
✓ DLL = original version (verify hash)
✓ bind.key deleted
✓ App X về trạng thái gốc
```

#### **TC4: Copy DLL sang máy khác**
```
Steps:
1. Máy A: Cắm USB → Patched
2. Copy App X + DLL patch sang máy B
3. Máy B: Chạy App X (KHÔNG có USB)

Expected:
✓ Máy B: DLL không tìm thấy bind.key → Disable features
✓ Máy B: App X chạy nhưng không có features mới

Alternative:
4. Copy cả bind.key sang máy B
5. Máy B: Chạy App X

Expected:
✓ bind.key verify FAIL (hardware mismatch)
✓ DLL self-destruct
✓ App X crash hoặc disable features
```

#### **TC5: Kill service process**
```
Steps:
1. Cắm USB → Patched
2. App X đang chạy với features
3. Kill service process (taskkill /F /IM DongleSyncService.exe)

Expected:
✓ DLL không nhận heartbeat sau 10s
✓ DLL self-destruct
✓ Features disabled
✓ App X vẫn chạy nhưng không có features
```

#### **TC6: Disconnect USB trong khi App X chạy**
```
Steps:
1. Cắm USB
2. Start App X
3. Đang dùng App X, rút USB đột ngột

Expected:
✓ Service detect trong 5s
✓ Service restore DLL (có thể fail vì App X đang lock file)
✓ DLL không nhận heartbeat → Self-destruct
✓ Features disabled
✓ Lần sau start App X → Back to normal
```

#### **TC7: VM Snapshot/Restore**
```
Steps:
1. VM: Cắm USB → Patched
2. Take VM snapshot
3. Rút USB
4. Restore snapshot

Expected:
✓ bind.key timestamp mismatch
✓ DLL detect "time travel"
✓ Self-destruct
✓ Features disabled
```

### 7.2 Test Cases cho Security

#### **SEC1: Reverse Engineering Protection**
```
Test:
1. Dump DLL từ memory (Process Explorer)
2. Decompile với dnSpy/ILSpy

Expected:
✓ Code obfuscated (unreadable)
✓ Strings encrypted
✓ Control flow mangled
✓ Không thể hiểu logic dễ dàng
```

#### **SEC2: Debugger Detection**
```
Test:
1. Attach debugger (Visual Studio/x64dbg)
2. Set breakpoint trong DLL

Expected:
✓ DLL detect debugger
✓ Self-destruct ngay lập tức
✓ App X crash hoặc disable features
```

#### **SEC3: Memory Dump Analysis**
```
Test:
1. Dump process memory (Process Hacker)
2. Search for secrets (keys, passwords, etc.)

Expected:
✓ Không tìm thấy plain-text keys
✓ Sensitive data được zero sau use
✓ Memory regions protected
```

#### **SEC4: USB Clone**
```
Test:
1. Clone USB sang USB khác (dd command)
2. Cắm USB clone

Expected:
✓ USB Serial Number khác
✓ dongle.key mismatch
✓ Validation FAIL
✓ Không patch DLL
```

#### **SEC5: Tamper Files in USB**
```
Test:
1. Modify patch.dll.enc trong USB
2. Cắm USB

Expected:
✓ HMAC checksum mismatch
✓ Validation FAIL
✓ Không patch DLL
```

---

## 📊 PHẦN 8: TIMELINE THỰC HIỆN (10 NGÀY)

### **Detailed Breakdown:**

```
╔════════════════════════════════════════════════════════════╗
║  NGÀY 1-2: Service Core & USB Detection          (16h)    ║
╠════════════════════════════════════════════════════════════╣
║  ├─ Setup project structure                      (2h)     ║
║  ├─ Implement USB detection (WMI)                (3h)     ║
║  ├─ Implement basic validation                   (3h)     ║
║  ├─ Create Windows Service framework             (4h)     ║
║  ├─ Setup logging                                (2h)     ║
║  └─ Basic testing                                (2h)     ║
╚════════════════════════════════════════════════════════════╝

╔════════════════════════════════════════════════════════════╗
║  NGÀY 3-4: Encryption & Security                 (16h)    ║
╠════════════════════════════════════════════════════════════╣
║  ├─ Implement AES-256 encryption                 (3h)     ║
║  ├─ Implement RSA-2048 signature                 (3h)     ║
║  ├─ Implement HMAC checksum                      (2h)     ║
║  ├─ Implement hardware fingerprinting            (3h)     ║
║  ├─ Implement bind.key system                    (3h)     ║
║  └─ Testing security layers                      (2h)     ║
╚════════════════════════════════════════════════════════════╝

╔════════════════════════════════════════════════════════════╗
║  NGÀY 5-6: DLL Management & IPC                  (16h)    ║
╠════════════════════════════════════════════════════════════╣
║  ├─ Implement App X location finder              (3h)     ║
║  ├─ Implement DLL backup/restore                 (3h)     ║
║  ├─ Implement DLL patch mechanism                (3h)     ║
║  ├─ Implement Named Pipe IPC                     (3h)     ║
║  ├─ Implement heartbeat monitor                  (2h)     ║
║  └─ Integration testing                          (2h)     ║
╚════════════════════════════════════════════════════════════╝

╔════════════════════════════════════════════════════════════╗
║  NGÀY 7: DLL Patch Protection                    (8h)     ║
╠════════════════════════════════════════════════════════════╣
║  ├─ Implement anti-debug checks                  (2h)     ║
║  ├─ Implement self-destruct mechanism            (2h)     ║
║  ├─ Implement IPC client in DLL                  (2h)     ║
║  └─ Implement continuous validation              (2h)     ║
╚════════════════════════════════════════════════════════════╝

╔════════════════════════════════════════════════════════════╗
║  NGÀY 8: Tools & Obfuscation                     (8h)     ║
╠════════════════════════════════════════════════════════════╣
║  ├─ Create USB Dongle Creator tool               (4h)     ║
║  ├─ Apply ConfuserEx obfuscation                 (2h)     ║
║  └─ Testing obfuscated code                      (2h)     ║
╚════════════════════════════════════════════════════════════╝

╔════════════════════════════════════════════════════════════╗
║  NGÀY 9: Installer & Integration                 (8h)     ║
╠════════════════════════════════════════════════════════════╣
║  ├─ Create WiX installer project                 (3h)     ║
║  ├─ Test installation/uninstallation             (2h)     ║
║  └─ End-to-end integration testing               (3h)     ║
╚════════════════════════════════════════════════════════════╝

╔════════════════════════════════════════════════════════════╗
║  NGÀY 10: Testing & Documentation                (8h)     ║
╠════════════════════════════════════════════════════════════╣
║  ├─ Security testing (all attack vectors)        (3h)     ║
║  ├─ Performance testing                          (1h)     ║
║  ├─ Write user documentation                     (2h)     ║
║  ├─ Write technical documentation                (1h)     ║
║  └─ Final verification & packaging               (1h)     ║
╚════════════════════════════════════════════════════════════╝

TOTAL: 80 giờ = 10 ngày (8 giờ/ngày)
```

---

## 📚 PHẦN 9: TÀI LIỆU THAM KHẢO

### 9.1 Technologies & Libraries

```
.NET 6.0
├── System.Management (WMI)
├── System.Security.Cryptography (AES, RSA, HMAC)
├── System.IO.Pipes (Named Pipes IPC)
└── TopShelf (Windows Service framework)

Security
├── ConfuserEx (Code obfuscation)
├── PBKDF2 (Key derivation)
└── AES-256-GCM (Authenticated encryption)

Installer
└── WiX Toolset 3.11 (MSI installer)
```

### 9.2 Best Practices

```
✓ Always use authenticated encryption (GCM mode)
✓ Use PBKDF2 với ít nhất 100,000 iterations
✓ Secure erase sensitive data (7-pass DoD standard)
✓ Log security events
✓ Implement rate limiting
✓ Use constant-time comparison for secrets
✓ Validate all inputs
✓ Principle of least privilege
```

### 9.3 Common Pitfalls

```
❌ Lưu keys trong plain text
❌ Sử dụng weak encryption (DES, RC4)
❌ Không validate inputs
❌ Hardcode secrets trong code
❌ Không handle edge cases
❌ Tin tưởng client-side validation
❌ Không log security events
```

---

## 🎯 PHẦN 10: KẾT LUẬN

### 10.1 Tóm tắt

Hệ thống USB Dongle này cung cấp:

```
✅ Bảo mật 7 lớp (USB → DLL → Machine → Runtime → Memory)
✅ Chống leak hiệu quả (copy USB/DLL/machine đều fail)
✅ Tự động 100% (user chỉ cắm/rút USB)
✅ Developer-friendly (dev mode)
✅ Production-ready (installer + service)
✅ Maintainable (logging + monitoring)
```

### 10.2 Security Level

```
Security Rating: ⭐⭐⭐⭐⭐ (9/10)

Có thể bị phá:
├── ⚠️ Skilled reverse engineer + nhiều thời gian (tuần/tháng)
├── ⚠️ Hardware debugger + JTAG attack
└── ⚠️ Kernel-mode rootkit

Nhưng:
├── ✅ 99.9% users không thể phá
├── ✅ Amateur hackers không thể phá
├── ✅ Copy/clone/VM đều fail
└── ✅ Cost to crack >> giá trị phần mềm
```

### 10.3 Trade-offs

```
Advantages:
✅ Bảo mật cao
✅ User-friendly
✅ Tự động hoàn toàn
✅ Không modify App X
✅ Dễ deploy

Disadvantages:
⚠️ Phức tạp (10 ngày dev)
⚠️ Yêu cầu Windows Service
⚠️ USB có thể bị mất
⚠️ Performance overhead nhỏ (heartbeat 5s)
```

### 10.4 Recommendation

```
✅ Dùng cho commercial software có giá trị
✅ Dùng cho enterprise licenses
✅ Dùng khi cần anti-piracy mạnh

❌ Overkill cho hobby projects
❌ Không phù hợp cho mass-market software
❌ Không phù hợp khi user không tech-savvy
```

---

## 📞 PHẦN 11: SUPPORT & MAINTENANCE

### 11.1 Troubleshooting Guide

```
Issue: USB không được detect
Solution:
├── Check WMI service running
├── Check USB có dongle folder
├── Check service logs
└── Verify USB permissions

Issue: DLL patch fail
Solution:
├── Check App X có đang chạy không
├── Check admin permissions
├── Check disk space
├── Verify DLL backup exists
└── Check logs chi tiết

Issue: Features không hoạt động sau patch
Solution:
├── Check bind.key exists
├── Check IPC connection
├── Check heartbeat logs
├── Verify USB còn cắm
└── Check for debugger

Issue: Service không start
Solution:
├── Check .NET 6.0 installed
├── Check Windows Event Log
├── Verify service account permissions
├── Check port conflicts
└── Reinstall service
```

### 11.2 Maintenance Tasks

```
Weekly:
├── Review security logs
├── Check service health
└── Monitor disk space

Monthly:
├── Update security patches
├── Review access logs
├── Backup RSA keys
└── Test disaster recovery

Quarterly:
├── Security audit
├── Performance review
├── Update documentation
└── Review attack vectors
```

### 11.3 Update Strategy

```
Service Update:
1. Build new version
2. Test thoroughly
3. Create installer
4. Deploy to test machines
5. Verify backward compatibility
6. Roll out to production
7. Monitor for issues

USB Dongle Update:
1. Create new DLL version
2. Test with existing USB
3. Option A: Update existing USB (re-encrypt)
4. Option B: Create new USB dongle
5. Notify users
6. Support both old + new versions (transition period)
```

---

## ✅ CHECKLIST HOÀN CHỈNH

```
PHASE 1: Development
├── [  ] Setup development environment
├── [  ] Implement all 7 security layers
├── [  ] Create USB Dongle Creator tool
├── [  ] Implement dev mode
├── [  ] Apply code obfuscation
├── [  ] Create installer
└── [  ] Complete all test cases

PHASE 2: Testing
├── [  ] Functional testing (all scenarios)
├── [  ] Security testing (all attack vectors)
├── [  ] Performance testing
├── [  ] User acceptance testing
└── [  ] Stress testing

PHASE 3: Documentation
├── [  ] Technical documentation
├── [  ] User guide
├── [  ] Installation guide
├── [  ] Troubleshooting guide
└── [  ] API documentation (if needed)

PHASE 4: Deployment
├── [  ] Create master USB dongles
├── [  ] Package installer
├── [  ] Prepare support materials
├── [  ] Train support team
└── [  ] Deploy to production

PHASE 5: Monitoring
├── [  ] Setup logging infrastructure
├── [  ] Monitor service health
├── [  ] Track security events
├── [  ] Collect user feedback
└── [  ] Plan updates
```

---

**🎉 TÀI LIỆU HOÀN CHỈNH - SẴN SÀNG TRIỂN KHAI!**

---

_Last updated: December 3, 2025_  
_Document version: 1.0.0_  
_Classification: Internal Use Only_

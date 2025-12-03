# 🚀 HỆ THỐNG USB DONGLE 4 LAYERS - QUY TRÌNH CHI TIẾT 6 NGÀY

## 📌 TỔNG QUAN

### Mục tiêu
- ✅ 1 người developer
- ✅ 6 ngày làm việc (8 giờ/ngày)
- ✅ 4 layers bảo mật cốt lõi
- ✅ Đơn giản nhưng hiệu quả

### 4 Layers bảo mật

```
Layer 1: USB Hardware ID Validation
└── Chống copy USB sang USB khác

Layer 2: AES-256 Encrypted DLL
└── DLL trong USB được mã hóa, không đọc được

Layer 3: Machine Binding
└── Chống copy app sang máy khác

Layer 4: Runtime Heartbeat Monitor
└── Rút USB → tự động restore trong 5s
```

### Yêu cầu đáp ứng

```
✅ Copy USB → FAIL (Hardware ID khác)
✅ Copy DLL → FAIL (Machine binding)
✅ Copy sang máy khác → FAIL (Hardware fingerprint)
✅ Rút USB → Auto restore ngay lập tức
✅ Developer test → Dev mode
```

---

## 📂 CẤU TRÚC DỰ ÁN

```
DongleSystem/
├── DongleSyncService/              # Windows Service (Main)
│   ├── Program.cs                  # Entry point + TopShelf config
│   ├── DongleService.cs            # Service logic
│   ├── Services/
│   │   ├── USBMonitor.cs           # USB detection (WMI)
│   │   ├── USBValidator.cs         # USB Hardware ID validation
│   │   ├── CryptoService.cs        # AES encryption/decryption
│   │   ├── MachineBindingService.cs # Hardware fingerprint
│   │   ├── DLLManager.cs           # Backup/Patch/Restore DLL
│   │   ├── IPCServer.cs            # Named Pipe IPC
│   │   ├── HeartbeatMonitor.cs     # Runtime check USB
│   │   └── StateManager.cs         # State persistence
│   ├── Models/
│   │   ├── ServiceState.cs
│   │   ├── DongleConfig.cs
│   │   └── BindingData.cs
│   └── Utils/
│       ├── Constants.cs
│       ├── Logger.cs
│       └── FileHelper.cs
│
├── DLLPatch/                       # DLL modifications
│   ├── DLLEntryPoint.cs            # Hook vào App X
│   ├── BindingValidator.cs         # Check bind.key
│   ├── IPCClient.cs                # IPC với service
│   └── FeatureController.cs        # Enable/disable features
│
├── DongleCreatorTool/              # USB creator (WinForms)
│   ├── MainForm.cs                 # UI chính
│   ├── USBWriter.cs                # Ghi data vào USB
│   └── CryptoHelper.cs             # Encryption logic
│
├── Installer/                      # WiX installer
│   └── Product.wxs                 # Installer definition
│
└── Tests/                          # Unit tests (optional)
    └── BasicTests.cs
```

---

## 🗓️ NGÀY 1: SERVICE CORE & USB DETECTION (8 giờ)

### ⏰ 09:00 - 09:30 | Setup Project (30 phút)

#### **Bước 1: Tạo Solution**

```powershell
# Mở PowerShell tại workspace
cd f:\3.Laptrinh\DUANUSB2

# Tạo folder structure
mkdir src
cd src

# Tạo solution
dotnet new sln -n DongleSystem

# Tạo projects
dotnet new console -n DongleSyncService -f net6.0
dotnet new classlib -n DLLPatch -f net6.0
dotnet new winforms -n DongleCreatorTool -f net6.0

# Add vào solution
dotnet sln add DongleSyncService/DongleSyncService.csproj
dotnet sln add DLLPatch/DLLPatch.csproj
dotnet sln add DongleCreatorTool/DongleCreatorTool.csproj

# Test build
dotnet build
```

#### **Bước 2: Install NuGet Packages**

```powershell
# DongleSyncService
cd DongleSyncService
dotnet add package Topshelf
dotnet add package Serilog
dotnet add package Serilog.Sinks.File
dotnet add package System.Management
dotnet add package Newtonsoft.Json

cd ..
```

**✅ Checkpoint**: Solution build thành công

---

### ⏰ 09:30 - 10:30 | Constants & Models (1 giờ)

#### **File 1: `Utils/Constants.cs`**

```csharp
namespace DongleSyncService.Utils
{
    public static class Constants
    {
        // Service
        public const string ServiceName = "DongleSyncService";
        public const string ServiceDisplayName = "USB Dongle Sync Service";
        
        // Paths
        public const string ProgramData = @"C:\ProgramData\DongleSyncService";
        public const string LogFolder = @"C:\ProgramData\DongleSyncService\logs";
        public const string ConfigFile = @"C:\ProgramData\DongleSyncService\config.json";
        public const string StateFile = @"C:\ProgramData\DongleSyncService\state.json";
        public const string BindKeyFile = @"C:\ProgramData\DongleSyncService\bind.key";
        public const string DevModeFile = @"C:\ProgramData\DongleSyncService\devmode.json";
        public const string BackupFolder = @"C:\ProgramData\DongleSyncService\backups";
        
        // USB Dongle
        public const string DongleFolderName = "dongle";
        public const string DongleConfigFile = "config.json";
        public const string DongleKeyFile = "dongle.key";
        public const string PatchDllFile = "patch.dll.enc";
        public const string PatchIvFile = "iv.bin";
        
        // Target
        public const string TargetDllName = "CHC.CGO.Common.dll";
        
        // IPC
        public const string PipeName = "DongleSyncPipe";
        
        // Timing
        public const int HeartbeatInterval = 5000; // 5 seconds
        public const int IPCTimeout = 10000; // 10 seconds
    }
}
```

#### **File 2: `Models/ServiceState.cs`**

```csharp
using Newtonsoft.Json;

namespace DongleSyncService.Models
{
    public class ServiceState
    {
        [JsonProperty("isPatched")]
        public bool IsPatched { get; set; }
        
        [JsonProperty("usbGuid")]
        public string UsbGuid { get; set; }
        
        [JsonProperty("dllPath")]
        public string DllPath { get; set; }
        
        [JsonProperty("machineId")]
        public string MachineId { get; set; }
        
        [JsonProperty("lastPatchTime")]
        public DateTime? LastPatchTime { get; set; }
        
        [JsonProperty("lastRestoreTime")]
        public DateTime? LastRestoreTime { get; set; }
    }
}
```

#### **File 3: `Models/DongleConfig.cs`**

```csharp
using Newtonsoft.Json;

namespace DongleSyncService.Models
{
    public class DongleConfig
    {
        [JsonProperty("usbGuid")]
        public string UsbGuid { get; set; }
        
        [JsonProperty("version")]
        public string Version { get; set; }
        
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
```

#### **File 4: `Models/BindingData.cs`**

```csharp
using Newtonsoft.Json;

namespace DongleSyncService.Models
{
    public class BindingData
    {
        [JsonProperty("usbGuid")]
        public string UsbGuid { get; set; }
        
        [JsonProperty("machineFingerprint")]
        public string MachineFingerprint { get; set; }
        
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
```

**✅ Checkpoint**: Models compile thành công

---

### ⏰ 10:30 - 12:00 | USB Detection (1.5 giờ)

#### **File: `Services/USBMonitor.cs`**

```csharp
using System.Management;
using Serilog;

namespace DongleSyncService.Services
{
    public class USBMonitor : IDisposable
    {
        private ManagementEventWatcher _insertWatcher;
        private ManagementEventWatcher _removeWatcher;
        private bool _isRunning;
        
        public event EventHandler<USBEventArgs> USBInserted;
        public event EventHandler<USBEventArgs> USBRemoved;

        public void Start()
        {
            try
            {
                Log.Information("Starting USB Monitor...");
                
                // Watch for USB insertion (EventType = 2)
                var insertQuery = new WqlEventQuery(
                    "SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2"
                );
                _insertWatcher = new ManagementEventWatcher(insertQuery);
                _insertWatcher.EventArrived += OnUSBInserted;
                _insertWatcher.Start();
                
                // Watch for USB removal (EventType = 3)
                var removeQuery = new WqlEventQuery(
                    "SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 3"
                );
                _removeWatcher = new ManagementEventWatcher(removeQuery);
                _removeWatcher.EventArrived += OnUSBRemoved;
                _removeWatcher.Start();
                
                _isRunning = true;
                Log.Information("USB Monitor started successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to start USB Monitor");
                throw;
            }
        }

        public void Stop()
        {
            try
            {
                _isRunning = false;
                _insertWatcher?.Stop();
                _removeWatcher?.Stop();
                Log.Information("USB Monitor stopped");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error stopping USB Monitor");
            }
        }

        private void OnUSBInserted(object sender, EventArrivedEventArgs e)
        {
            try
            {
                var driveName = GetDriveFromEvent(e);
                if (!string.IsNullOrEmpty(driveName))
                {
                    Log.Information("USB inserted: {Drive}", driveName);
                    USBInserted?.Invoke(this, new USBEventArgs(driveName));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling USB insertion");
            }
        }

        private void OnUSBRemoved(object sender, EventArrivedEventArgs e)
        {
            try
            {
                var driveName = GetDriveFromEvent(e);
                if (!string.IsNullOrEmpty(driveName))
                {
                    Log.Information("USB removed: {Drive}", driveName);
                    USBRemoved?.Invoke(this, new USBEventArgs(driveName));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling USB removal");
            }
        }

        private string GetDriveFromEvent(EventArrivedEventArgs e)
        {
            try
            {
                return e.NewEvent.Properties["DriveName"].Value?.ToString();
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            Stop();
            _insertWatcher?.Dispose();
            _removeWatcher?.Dispose();
        }
    }

    public class USBEventArgs : EventArgs
    {
        public string DriveName { get; }
        
        public USBEventArgs(string driveName)
        {
            DriveName = driveName;
        }
    }
}
```

**✅ Checkpoint**: USB Monitor compile thành công

---

### ⏰ 12:00 - 13:00 | 🍽️ NGHỈ TRƯA

---

### ⏰ 13:00 - 15:00 | USB Hardware ID Validation (2 giờ)

#### **File: `Services/USBValidator.cs`**

```csharp
using System.Management;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Serilog;
using DongleSyncService.Models;
using DongleSyncService.Utils;

namespace DongleSyncService.Services
{
    public class USBValidator
    {
        public bool ValidateDongle(string drivePath, out DongleConfig config)
        {
            config = null;

            try
            {
                Log.Information("Validating dongle on drive: {Drive}", drivePath);

                // 1. Check dongle folder exists
                var donglePath = Path.Combine(drivePath, Constants.DongleFolderName);
                if (!Directory.Exists(donglePath))
                {
                    Log.Warning("Dongle folder not found: {Path}", donglePath);
                    return false;
                }

                // 2. Check required files
                var configPath = Path.Combine(donglePath, Constants.DongleConfigFile);
                var keyPath = Path.Combine(donglePath, Constants.DongleKeyFile);
                var patchPath = Path.Combine(donglePath, Constants.PatchDllFile);
                var ivPath = Path.Combine(donglePath, Constants.PatchIvFile);

                if (!File.Exists(configPath) || !File.Exists(keyPath) || 
                    !File.Exists(patchPath) || !File.Exists(ivPath))
                {
                    Log.Warning("Required files missing in dongle");
                    return false;
                }

                // 3. Load config
                var configJson = File.ReadAllText(configPath);
                config = JsonConvert.DeserializeObject<DongleConfig>(configJson);

                if (config == null || string.IsNullOrEmpty(config.UsbGuid))
                {
                    Log.Warning("Invalid config file");
                    return false;
                }

                // 4. Validate USB Hardware ID
                var expectedKey = File.ReadAllText(keyPath).Trim();
                var actualKey = ComputeUSBHardwareKey(drivePath);

                if (expectedKey != actualKey)
                {
                    Log.Warning("USB Hardware ID mismatch");
                    Log.Debug("Expected: {Expected}, Actual: {Actual}", expectedKey, actualKey);
                    return false;
                }

                Log.Information("Dongle validation successful. GUID: {Guid}", config.UsbGuid);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error validating dongle");
                return false;
            }
        }

        public string ComputeUSBHardwareKey(string drivePath)
        {
            try
            {
                // Get USB Serial Number
                var drive = drivePath.TrimEnd('\\');
                var query = $"SELECT * FROM Win32_LogicalDisk WHERE DeviceID = '{drive}'";
                
                using var searcher = new ManagementObjectSearcher(query);
                var collection = searcher.Get();
                
                string volumeSerial = "";
                foreach (ManagementObject obj in collection)
                {
                    volumeSerial = obj["VolumeSerialNumber"]?.ToString() ?? "";
                    break;
                }

                // Get Device ID
                var deviceQuery = $"SELECT * FROM Win32_DiskDrive WHERE Size IS NOT NULL";
                using var deviceSearcher = new ManagementObjectSearcher(deviceQuery);
                var deviceCollection = deviceSearcher.Get();
                
                string deviceId = "";
                foreach (ManagementObject obj in deviceCollection)
                {
                    var serialNumber = obj["SerialNumber"]?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(serialNumber))
                    {
                        deviceId = serialNumber;
                        break;
                    }
                }

                // Combine and hash
                var combined = $"{volumeSerial}|{deviceId}";
                using var sha256 = SHA256.Create();
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                return Convert.ToBase64String(hashBytes);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error computing USB hardware key");
                return "";
            }
        }

        public string GetDonglePath(string drivePath)
        {
            return Path.Combine(drivePath, Constants.DongleFolderName);
        }
    }
}
```

**✅ Checkpoint**: USB Validator compile và test

---

### ⏰ 15:00 - 17:00 | State Manager & Service Skeleton (2 giờ)

#### **File: `Services/StateManager.cs`**

```csharp
using Newtonsoft.Json;
using Serilog;
using DongleSyncService.Models;
using DongleSyncService.Utils;

namespace DongleSyncService.Services
{
    public class StateManager
    {
        private ServiceState _state;
        private readonly object _lock = new object();

        public StateManager()
        {
            EnsureDirectories();
            LoadState();
        }

        private void EnsureDirectories()
        {
            Directory.CreateDirectory(Constants.ProgramData);
            Directory.CreateDirectory(Constants.LogFolder);
            Directory.CreateDirectory(Constants.BackupFolder);
        }

        public ServiceState GetState()
        {
            lock (_lock)
            {
                return _state;
            }
        }

        public void UpdateState(Action<ServiceState> updateAction)
        {
            lock (_lock)
            {
                updateAction(_state);
                SaveState();
            }
        }

        private void LoadState()
        {
            try
            {
                if (File.Exists(Constants.StateFile))
                {
                    var json = File.ReadAllText(Constants.StateFile);
                    _state = JsonConvert.DeserializeObject<ServiceState>(json);
                    Log.Information("State loaded successfully");
                }
                else
                {
                    _state = new ServiceState();
                    SaveState();
                    Log.Information("New state created");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load state, creating new");
                _state = new ServiceState();
            }
        }

        private void SaveState()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_state, Formatting.Indented);
                File.WriteAllText(Constants.StateFile, json);
                Log.Debug("State saved successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save state");
            }
        }
    }
}
```

#### **File: `DongleService.cs`**

```csharp
using Serilog;
using DongleSyncService.Services;

namespace DongleSyncService
{
    public class DongleService
    {
        private USBMonitor _usbMonitor;
        private USBValidator _validator;
        private StateManager _stateManager;

        public bool Start()
        {
            try
            {
                Log.Information("========================================");
                Log.Information("Dongle Service Starting...");
                Log.Information("========================================");
                
                // Initialize components
                _stateManager = new StateManager();
                _validator = new USBValidator();
                _usbMonitor = new USBMonitor();
                
                // Subscribe to USB events
                _usbMonitor.USBInserted += OnUSBInserted;
                _usbMonitor.USBRemoved += OnUSBRemoved;
                
                // Start monitoring
                _usbMonitor.Start();
                
                Log.Information("Dongle Service started successfully");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to start Dongle Service");
                return false;
            }
        }

        public bool Stop()
        {
            try
            {
                Log.Information("Dongle Service stopping...");
                
                if (_usbMonitor != null)
                {
                    _usbMonitor.USBInserted -= OnUSBInserted;
                    _usbMonitor.USBRemoved -= OnUSBRemoved;
                    _usbMonitor.Stop();
                    _usbMonitor.Dispose();
                }
                
                Log.Information("Dongle Service stopped");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error stopping Dongle Service");
                return false;
            }
        }

        private void OnUSBInserted(object sender, USBEventArgs e)
        {
            Log.Information("Processing USB insertion: {Drive}", e.DriveName);
            
            // Validate dongle
            if (!_validator.ValidateDongle(e.DriveName, out var config))
            {
                Log.Information("USB {Drive} is not a valid dongle", e.DriveName);
                return;
            }

            Log.Information("Valid dongle detected! GUID: {Guid}", config.UsbGuid);
            
            // TODO: Implement patching logic tomorrow
        }

        private void OnUSBRemoved(object sender, USBEventArgs e)
        {
            Log.Information("Processing USB removal: {Drive}", e.DriveName);
            
            // TODO: Implement restore logic tomorrow
        }
    }
}
```

#### **File: `Program.cs`**

```csharp
using Topshelf;
using Serilog;
using DongleSyncService.Utils;

namespace DongleSyncService
{
    class Program
    {
        static void Main(string[] args)
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    Path.Combine(Constants.LogFolder, "service-.log"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                var exitCode = HostFactory.Run(config =>
                {
                    config.Service<DongleService>(service =>
                    {
                        service.ConstructUsing(name => new DongleService());
                        service.WhenStarted(s => s.Start());
                        service.WhenStopped(s => s.Stop());
                    });

                    config.RunAsLocalSystem();
                    config.SetServiceName(Constants.ServiceName);
                    config.SetDisplayName(Constants.ServiceDisplayName);
                    config.SetDescription("Manages USB dongle for App X licensing");
                    config.StartAutomatically();
                    
                    config.EnableServiceRecovery(recovery =>
                    {
                        recovery.RestartService(1);
                    });

                    config.UseSerilog();
                });

                Environment.ExitCode = (int)exitCode;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Service terminated unexpectedly");
                Environment.ExitCode = 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
```

#### **Test chạy:**

```powershell
cd f:\3.Laptrinh\DUANUSB2\src\DongleSyncService

# Build
dotnet build

# Run console mode để test
dotnet run

# Cắm USB để xem log
# Expected output:
# USB inserted: E:\
# Validating dongle on drive: E:\
# (Pass hoặc fail tùy USB)
```

**✅ Checkpoint NGÀY 1**: 
- Service framework hoạt động
- USB detection hoạt động
- Hardware ID validation hoạt động

---

## 🗓️ NGÀY 2: ENCRYPTION & MACHINE BINDING (8 giờ)

### ⏰ 09:00 - 12:00 | AES-256 Encryption Service (3 giờ)

#### **File: `Services/CryptoService.cs`**

```csharp
using System.Security.Cryptography;
using System.Text;
using Serilog;

namespace DongleSyncService.Services
{
    public class CryptoService
    {
        private const int KeySize = 256;
        private const int Iterations = 100000;

        /// <summary>
        /// Decrypt DLL file from USB dongle
        /// </summary>
        public byte[] DecryptDLL(string encryptedPath, string ivPath, string usbSerial)
        {
            try
            {
                Log.Information("Decrypting DLL from: {Path}", encryptedPath);

                // Read encrypted data
                var encryptedData = File.ReadAllBytes(encryptedPath);
                var iv = File.ReadAllBytes(ivPath);

                // Derive key from USB serial
                var key = DeriveKey(usbSerial);

                // Decrypt
                using var aes = Aes.Create();
                aes.KeySize = KeySize;
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var decryptor = aes.CreateDecryptor();
                var decryptedData = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

                Log.Information("DLL decrypted successfully. Size: {Size} bytes", decryptedData.Length);
                return decryptedData;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to decrypt DLL");
                throw;
            }
        }

        /// <summary>
        /// Encrypt DLL file for USB dongle
        /// </summary>
        public (byte[] encrypted, byte[] iv) EncryptDLL(byte[] dllData, string usbSerial)
        {
            try
            {
                Log.Information("Encrypting DLL. Size: {Size} bytes", dllData.Length);

                // Derive key
                var key = DeriveKey(usbSerial);

                // Generate random IV
                using var aes = Aes.Create();
                aes.KeySize = KeySize;
                aes.Key = key;
                aes.GenerateIV();
                var iv = aes.IV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Encrypt
                using var encryptor = aes.CreateEncryptor();
                var encrypted = encryptor.TransformFinalBlock(dllData, 0, dllData.Length);

                Log.Information("DLL encrypted successfully");
                return (encrypted, iv);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to encrypt DLL");
                throw;
            }
        }

        /// <summary>
        /// Derive encryption key from USB serial using PBKDF2
        /// </summary>
        private byte[] DeriveKey(string usbSerial)
        {
            // Master secret (hardcoded - would be obfuscated in production)
            const string masterSecret = "DongleSecretKey2025!@#$%^&*()";
            
            // Combine USB serial with master secret
            var password = $"{usbSerial}|{masterSecret}";
            
            // Use USB serial as salt
            var salt = Encoding.UTF8.GetBytes(usbSerial);
            
            // Derive key using PBKDF2
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256
            );
            
            return pbkdf2.GetBytes(KeySize / 8);
        }

        /// <summary>
        /// Encrypt bind key data
        /// </summary>
        public byte[] EncryptBindKey(string json, string machineId)
        {
            try
            {
                var data = Encoding.UTF8.GetBytes(json);
                var key = DeriveKey(machineId);

                using var aes = Aes.Create();
                aes.KeySize = KeySize;
                aes.Key = key;
                aes.GenerateIV();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var encryptor = aes.CreateEncryptor();
                var encrypted = encryptor.TransformFinalBlock(data, 0, data.Length);

                // Prepend IV to encrypted data
                var result = new byte[aes.IV.Length + encrypted.Length];
                Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
                Buffer.BlockCopy(encrypted, 0, result, aes.IV.Length, encrypted.Length);

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to encrypt bind key");
                throw;
            }
        }

        /// <summary>
        /// Decrypt bind key data
        /// </summary>
        public string DecryptBindKey(byte[] encryptedData, string machineId)
        {
            try
            {
                var key = DeriveKey(machineId);

                using var aes = Aes.Create();
                aes.KeySize = KeySize;
                aes.Key = key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Extract IV from beginning
                var iv = new byte[aes.BlockSize / 8];
                Buffer.BlockCopy(encryptedData, 0, iv, 0, iv.Length);
                aes.IV = iv;

                // Extract encrypted data
                var encrypted = new byte[encryptedData.Length - iv.Length];
                Buffer.BlockCopy(encryptedData, iv.Length, encrypted, 0, encrypted.Length);

                // Decrypt
                using var decryptor = aes.CreateDecryptor();
                var decrypted = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);

                return Encoding.UTF8.GetString(decrypted);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to decrypt bind key");
                throw;
            }
        }
    }
}
```

**✅ Checkpoint**: CryptoService compile thành công

---

### ⏰ 12:00 - 13:00 | 🍽️ NGHỈ TRƯA

---

### ⏰ 13:00 - 16:00 | Machine Binding Service (3 giờ)

#### **File: `Services/MachineBindingService.cs`**

```csharp
using System.Management;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Serilog;
using DongleSyncService.Models;
using DongleSyncService.Utils;

namespace DongleSyncService.Services
{
    public class MachineBindingService
    {
        private readonly CryptoService _crypto;

        public MachineBindingService(CryptoService crypto)
        {
            _crypto = crypto;
        }

        /// <summary>
        /// Get unique machine fingerprint
        /// </summary>
        public string GetMachineFingerprint()
        {
            try
            {
                Log.Debug("Computing machine fingerprint...");

                var cpuId = GetCPUID();
                var biosSerial = GetBIOSSerial();
                var diskSerial = GetDiskSerial();
                var macAddress = GetMACAddress();

                var combined = $"{cpuId}|{biosSerial}|{diskSerial}|{macAddress}";
                
                using var sha256 = SHA256.Create();
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                var fingerprint = Convert.ToBase64String(hash);

                Log.Debug("Machine fingerprint: {Fingerprint}", fingerprint.Substring(0, 16) + "...");
                return fingerprint;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to compute machine fingerprint");
                throw;
            }
        }

        /// <summary>
        /// Create binding between USB and machine
        /// </summary>
        public void CreateBinding(string usbGuid)
        {
            try
            {
                Log.Information("Creating machine binding for USB: {Guid}", usbGuid);

                var machineId = GetMachineFingerprint();
                
                var binding = new BindingData
                {
                    UsbGuid = usbGuid,
                    MachineFingerprint = machineId,
                    Timestamp = DateTime.UtcNow
                };

                var json = JsonConvert.SerializeObject(binding);
                var encrypted = _crypto.EncryptBindKey(json, machineId);

                File.WriteAllBytes(Constants.BindKeyFile, encrypted);

                Log.Information("Binding created successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create binding");
                throw;
            }
        }

        /// <summary>
        /// Validate existing binding
        /// </summary>
        public bool ValidateBinding(string expectedUsbGuid)
        {
            try
            {
                if (!File.Exists(Constants.BindKeyFile))
                {
                    Log.Warning("Bind key file not found");
                    return false;
                }

                var encrypted = File.ReadAllBytes(Constants.BindKeyFile);
                var machineId = GetMachineFingerprint();
                var json = _crypto.DecryptBindKey(encrypted, machineId);
                var binding = JsonConvert.DeserializeObject<BindingData>(json);

                if (binding == null)
                {
                    Log.Warning("Failed to deserialize binding data");
                    return false;
                }

                // Check USB GUID match
                if (binding.UsbGuid != expectedUsbGuid)
                {
                    Log.Warning("USB GUID mismatch in binding");
                    return false;
                }

                // Check machine fingerprint match
                if (binding.MachineFingerprint != machineId)
                {
                    Log.Warning("Machine fingerprint mismatch");
                    return false;
                }

                // Check timestamp (prevent time travel)
                if (DateTime.UtcNow < binding.Timestamp)
                {
                    Log.Warning("Binding timestamp is in the future (time travel detected)");
                    return false;
                }

                Log.Information("Binding validation successful");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to validate binding");
                return false;
            }
        }

        /// <summary>
        /// Delete binding (when USB removed)
        /// </summary>
        public void DeleteBinding()
        {
            try
            {
                if (File.Exists(Constants.BindKeyFile))
                {
                    // Secure erase (overwrite with random data)
                    var fileInfo = new FileInfo(Constants.BindKeyFile);
                    var length = fileInfo.Length;
                    
                    using (var fs = new FileStream(Constants.BindKeyFile, FileMode.Open))
                    {
                        var random = new byte[length];
                        RandomNumberGenerator.Fill(random);
                        fs.Write(random, 0, random.Length);
                        fs.Flush();
                    }

                    File.Delete(Constants.BindKeyFile);
                    Log.Information("Binding deleted successfully");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete binding");
            }
        }

        // Hardware ID collection methods
        private string GetCPUID()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
                foreach (ManagementObject obj in searcher.Get())
                {
                    return obj["ProcessorId"]?.ToString() ?? "";
                }
            }
            catch { }
            return "";
        }

        private string GetBIOSSerial()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS");
                foreach (ManagementObject obj in searcher.Get())
                {
                    return obj["SerialNumber"]?.ToString() ?? "";
                }
            }
            catch { }
            return "";
        }

        private string GetDiskSerial()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_PhysicalMedia");
                foreach (ManagementObject obj in searcher.Get())
                {
                    var serial = obj["SerialNumber"]?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(serial))
                        return serial;
                }
            }
            catch { }
            return "";
        }

        private string GetMACAddress()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT MACAddress FROM Win32_NetworkAdapter WHERE MACAddress IS NOT NULL");
                foreach (ManagementObject obj in searcher.Get())
                {
                    var mac = obj["MACAddress"]?.ToString();
                    if (!string.IsNullOrEmpty(mac))
                        return mac;
                }
            }
            catch { }
            return "";
        }
    }
}
```

**✅ Checkpoint**: Machine Binding compile thành công

---

### ⏰ 16:00 - 17:00 | Test Encryption & Binding (1 giờ)

#### **Test Console App:**

```csharp
// Thêm vào Program.cs để test
static void TestCrypto()
{
    var crypto = new CryptoService();
    var binding = new MachineBindingService(crypto);

    // Test machine fingerprint
    var fingerprint = binding.GetMachineFingerprint();
    Console.WriteLine($"Machine Fingerprint: {fingerprint}");

    // Test encryption
    var testData = Encoding.UTF8.GetBytes("Hello World");
    var (encrypted, iv) = crypto.EncryptDLL(testData, "TEST-USB-123");
    var decrypted = crypto.DecryptDLL(encrypted, iv, "TEST-USB-123");
    Console.WriteLine($"Decrypted: {Encoding.UTF8.GetString(decrypted)}");

    // Test binding
    binding.CreateBinding("test-guid-123");
    var valid = binding.ValidateBinding("test-guid-123");
    Console.WriteLine($"Binding valid: {valid}");
}
```

**✅ Checkpoint NGÀY 2**:
- AES-256 encryption hoạt động
- Machine binding hoạt động
- Validation works

---

## 🗓️ NGÀY 3: DLL MANAGEMENT (8 giờ)

### ⏰ 09:00 - 11:00 | App X Finder (2 giờ)

#### **File: `Services/AppFinder.cs`**

```csharp
using System.Text.RegularExpressions;
using Serilog;
using DongleSyncService.Utils;

namespace DongleSyncService.Services
{
    public class AppFinder
    {
        /// <summary>
        /// Find CHC.CGO.Common.dll in system
        /// </summary>
        public string FindTargetDLL()
        {
            try
            {
                Log.Information("Searching for target DLL: {DLL}", Constants.TargetDllName);

                // 1. Check cache first
                var cachedPath = GetCachedPath();
                if (!string.IsNullOrEmpty(cachedPath) && File.Exists(cachedPath))
                {
                    Log.Information("Using cached DLL path: {Path}", cachedPath);
                    return cachedPath;
                }

                // 2. Search common locations
                var searchPaths = new[]
                {
                    @"C:\Program Files",
                    @"C:\Program Files (x86)",
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    @"C:\Users"
                };

                foreach (var basePath in searchPaths.Where(Directory.Exists))
                {
                    Log.Debug("Searching in: {Path}", basePath);
                    
                    var foundPath = SearchDirectory(basePath, Constants.TargetDllName, maxDepth: 4);
                    if (!string.IsNullOrEmpty(foundPath))
                    {
                        CachePath(foundPath);
                        Log.Information("DLL found: {Path}", foundPath);
                        return foundPath;
                    }
                }

                Log.Warning("Target DLL not found");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error finding target DLL");
                return null;
            }
        }

        private string SearchDirectory(string path, string fileName, int maxDepth, int currentDepth = 0)
        {
            if (currentDepth > maxDepth) return null;

            try
            {
                // Search files in current directory
                var files = Directory.GetFiles(path, fileName, SearchOption.TopDirectoryOnly);
                if (files.Length > 0)
                {
                    // Filter out system/temp folders
                    var validFile = files.FirstOrDefault(f => IsValidPath(f));
                    if (validFile != null)
                        return validFile;
                }

                // Search subdirectories
                var directories = Directory.GetDirectories(path);
                foreach (var dir in directories)
                {
                    if (ShouldSkipDirectory(dir)) continue;

                    var result = SearchDirectory(dir, fileName, maxDepth, currentDepth + 1);
                    if (result != null) return result;
                }
            }
            catch
            {
                // Ignore access denied errors
            }

            return null;
        }

        private bool IsValidPath(string path)
        {
            var lowerPath = path.ToLowerInvariant();
            
            // Exclude system folders
            var excludePatterns = new[]
            {
                "\\windows\\",
                "\\winsxs\\",
                "\\temp\\",
                "\\cache\\",
                "\\backup\\",
                "\\$"
            };

            return !excludePatterns.Any(pattern => lowerPath.Contains(pattern));
        }

        private bool ShouldSkipDirectory(string directory)
        {
            var name = Path.GetFileName(directory).ToLowerInvariant();
            
            var skipList = new[]
            {
                "windows", "winsxs", "temp", "cache", 
                "$recycle.bin", "system volume information"
            };

            return skipList.Contains(name);
        }

        private string GetCachedPath()
        {
            try
            {
                var cacheFile = Path.Combine(Constants.ProgramData, "app_cache.txt");
                if (File.Exists(cacheFile))
                {
                    return File.ReadAllText(cacheFile).Trim();
                }
            }
            catch { }
            return null;
        }

        private void CachePath(string path)
        {
            try
            {
                var cacheFile = Path.Combine(Constants.ProgramData, "app_cache.txt");
                File.WriteAllText(cacheFile, path);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to cache DLL path");
            }
        }
    }
}
```

---

### ⏰ 11:00 - 12:00 | DLL Backup/Restore (1 giờ)

#### **File: `Services/DLLManager.cs` (Part 1)**

```csharp
using System.Security.Cryptography;
using System.Text;
using Serilog;
using DongleSyncService.Utils;

namespace DongleSyncService.Services
{
    public class DLLManager
    {
        private readonly CryptoService _crypto;
        private readonly AppFinder _appFinder;

        public DLLManager(CryptoService crypto, AppFinder appFinder)
        {
            _crypto = crypto;
            _appFinder = appFinder;
        }

        /// <summary>
        /// Backup original DLL
        /// </summary>
        public bool BackupDLL(string dllPath)
        {
            try
            {
                Log.Information("Backing up DLL: {Path}", dllPath);

                if (!File.Exists(dllPath))
                {
                    Log.Error("DLL file not found: {Path}", dllPath);
                    return false;
                }

                var backupPath = GetBackupPath(dllPath);
                
                // Don't backup if already exists (use existing backup)
                if (File.Exists(backupPath))
                {
                    Log.Information("Backup already exists: {Path}", backupPath);
                    return true;
                }

                // Copy file
                File.Copy(dllPath, backupPath, overwrite: false);

                // Save hash for verification
                var hash = ComputeFileHash(dllPath);
                File.WriteAllText(backupPath + ".hash", hash);

                Log.Information("Backup created successfully: {Path}", backupPath);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to backup DLL");
                return false;
            }
        }

        /// <summary>
        /// Restore original DLL from backup
        /// </summary>
        public bool RestoreDLL(string dllPath)
        {
            try
            {
                Log.Information("Restoring DLL: {Path}", dllPath);

                var backupPath = GetBackupPath(dllPath);
                
                if (!File.Exists(backupPath))
                {
                    Log.Error("Backup file not found: {Path}", backupPath);
                    return false;
                }

                // Verify backup integrity
                if (File.Exists(backupPath + ".hash"))
                {
                    var expectedHash = File.ReadAllText(backupPath + ".hash");
                    var actualHash = ComputeFileHash(backupPath);
                    
                    if (expectedHash != actualHash)
                    {
                        Log.Error("Backup file corrupted (hash mismatch)");
                        return false;
                    }
                }

                // Restore file
                File.Copy(backupPath, dllPath, overwrite: true);

                Log.Information("DLL restored successfully");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to restore DLL");
                return false;
            }
        }

        private string GetBackupPath(string dllPath)
        {
            var fileName = Path.GetFileName(dllPath);
            return Path.Combine(Constants.BackupFolder, fileName + ".bak");
        }

        private string ComputeFileHash(string filePath)
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hash = sha256.ComputeHash(stream);
            return Convert.ToBase64String(hash);
        }
    }
}
```

---

### ⏰ 12:00 - 13:00 | 🍽️ NGHỈ TRƯA

---

### ⏰ 13:00 - 16:00 | DLL Patching Logic (3 giờ)

#### **File: `Services/DLLManager.cs` (Part 2 - thêm vào class)**

```csharp
        /// <summary>
        /// Patch DLL with encrypted version from USB
        /// </summary>
        public bool PatchDLL(string donglePath, string usbSerial)
        {
            try
            {
                Log.Information("Patching DLL from dongle: {Path}", donglePath);

                // 1. Find target DLL
                var dllPath = _appFinder.FindTargetDLL();
                if (string.IsNullOrEmpty(dllPath))
                {
                    Log.Error("Target DLL not found");
                    return false;
                }

                // 2. Check if App X is running
                if (IsDLLInUse(dllPath))
                {
                    Log.Warning("DLL is currently in use. Please close App X first.");
                    return false;
                }

                // 3. Backup original DLL
                if (!BackupDLL(dllPath))
                {
                    Log.Error("Failed to backup DLL");
                    return false;
                }

                // 4. Decrypt patch DLL from USB
                var encPath = Path.Combine(donglePath, Constants.PatchDllFile);
                var ivPath = Path.Combine(donglePath, Constants.PatchIvFile);

                if (!File.Exists(encPath) || !File.Exists(ivPath))
                {
                    Log.Error("Patch files not found in dongle");
                    return false;
                }

                var decryptedDll = _crypto.DecryptDLL(encPath, ivPath, usbSerial);

                // 5. Write patched DLL
                File.WriteAllBytes(dllPath, decryptedDll);

                Log.Information("DLL patched successfully: {Path}", dllPath);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to patch DLL");
                
                // Try to restore backup on error
                try
                {
                    var dllPath = _appFinder.FindTargetDLL();
                    if (!string.IsNullOrEmpty(dllPath))
                    {
                        RestoreDLL(dllPath);
                    }
                }
                catch { }
                
                return false;
            }
        }

        /// <summary>
        /// Check if DLL is currently loaded by any process
        /// </summary>
        private bool IsDLLInUse(string dllPath)
        {
            try
            {
                // Try to open file with exclusive access
                using var fs = File.Open(dllPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return false;
            }
            catch (IOException)
            {
                // File is locked = in use
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get path of currently patched DLL
        /// </summary>
        public string GetPatchedDLLPath()
        {
            return _appFinder.FindTargetDLL();
        }
```

---

### ⏰ 16:00 - 17:00 | Integration & Testing (1 giờ)

#### **Update: `DongleService.cs` - Integrate DLL Manager**

```csharp
using Serilog;
using DongleSyncService.Services;

namespace DongleSyncService
{
    public class DongleService
    {
        private USBMonitor _usbMonitor;
        private USBValidator _validator;
        private StateManager _stateManager;
        private CryptoService _crypto;
        private MachineBindingService _binding;
        private AppFinder _appFinder;
        private DLLManager _dllManager;

        public bool Start()
        {
            try
            {
                Log.Information("========================================");
                Log.Information("Dongle Service Starting...");
                Log.Information("========================================");
                
                // Initialize components
                _stateManager = new StateManager();
                _crypto = new CryptoService();
                _binding = new MachineBindingService(_crypto);
                _appFinder = new AppFinder();
                _dllManager = new DLLManager(_crypto, _appFinder);
                _validator = new USBValidator();
                _usbMonitor = new USBMonitor();
                
                // Subscribe to USB events
                _usbMonitor.USBInserted += OnUSBInserted;
                _usbMonitor.USBRemoved += OnUSBRemoved;
                
                // Check if we're in a patched state without USB
                var state = _stateManager.GetState();
                if (state.IsPatched)
                {
                    Log.Warning("Service starting in patched state. Checking USB...");
                    // Will be handled by heartbeat monitor tomorrow
                }
                
                // Start monitoring
                _usbMonitor.Start();
                
                Log.Information("Dongle Service started successfully");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to start Dongle Service");
                return false;
            }
        }

        public bool Stop()
        {
            try
            {
                Log.Information("Dongle Service stopping...");
                
                if (_usbMonitor != null)
                {
                    _usbMonitor.USBInserted -= OnUSBInserted;
                    _usbMonitor.USBRemoved -= OnUSBRemoved;
                    _usbMonitor.Stop();
                    _usbMonitor.Dispose();
                }
                
                Log.Information("Dongle Service stopped");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error stopping Dongle Service");
                return false;
            }
        }

        private void OnUSBInserted(object sender, USBEventArgs e)
        {
            try
            {
                Log.Information("========================================");
                Log.Information("USB INSERTED: {Drive}", e.DriveName);
                Log.Information("========================================");
                
                // 1. Validate dongle
                if (!_validator.ValidateDongle(e.DriveName, out var config))
                {
                    Log.Information("USB {Drive} is not a valid dongle", e.DriveName);
                    return;
                }

                Log.Information("✓ Valid dongle detected! GUID: {Guid}", config.UsbGuid);
                
                // 2. Get USB hardware key for encryption
                var usbKey = _validator.ComputeUSBHardwareKey(e.DriveName);
                
                // 3. Patch DLL
                var donglePath = _validator.GetDonglePath(e.DriveName);
                if (!_dllManager.PatchDLL(donglePath, usbKey))
                {
                    Log.Error("Failed to patch DLL");
                    return;
                }

                Log.Information("✓ DLL patched successfully");

                // 4. Create machine binding
                _binding.CreateBinding(config.UsbGuid);
                Log.Information("✓ Machine binding created");

                // 5. Update state
                _stateManager.UpdateState(state =>
                {
                    state.IsPatched = true;
                    state.UsbGuid = config.UsbGuid;
                    state.DllPath = _dllManager.GetPatchedDLLPath();
                    state.MachineId = _binding.GetMachineFingerprint();
                    state.LastPatchTime = DateTime.UtcNow;
                });

                Log.Information("========================================");
                Log.Information("✓✓✓ USB DONGLE ACTIVATED SUCCESSFULLY!");
                Log.Information("========================================");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing USB insertion");
            }
        }

        private void OnUSBRemoved(object sender, USBEventArgs e)
        {
            try
            {
                Log.Information("========================================");
                Log.Information("USB REMOVED: {Drive}", e.DriveName);
                Log.Information("========================================");
                
                var state = _stateManager.GetState();
                
                if (!state.IsPatched)
                {
                    Log.Information("No patch active, nothing to restore");
                    return;
                }

                // 1. Restore DLL
                if (!string.IsNullOrEmpty(state.DllPath))
                {
                    _dllManager.RestoreDLL(state.DllPath);
                    Log.Information("✓ DLL restored");
                }

                // 2. Delete binding
                _binding.DeleteBinding();
                Log.Information("✓ Machine binding deleted");

                // 3. Update state
                _stateManager.UpdateState(s =>
                {
                    s.IsPatched = false;
                    s.UsbGuid = null;
                    s.LastRestoreTime = DateTime.UtcNow;
                });

                Log.Information("========================================");
                Log.Information("✓✓✓ USB DONGLE DEACTIVATED - APP RESTORED");
                Log.Information("========================================");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing USB removal");
            }
        }
    }
}
```

#### **Test end-to-end:**

```powershell
# Build
dotnet build

# Run service
dotnet run

# Test workflow:
# 1. Cắm USB dongle (chuẩn bị ở ngày 5)
# 2. Xem log - expect: DLL patched
# 3. Rút USB
# 4. Xem log - expect: DLL restored
```

**✅ Checkpoint NGÀY 3**:
- App X finder hoạt động
- DLL backup/restore hoạt động
- DLL patching hoạt động
- End-to-end: Cắm USB → Patch → Rút USB → Restore ✅

---

**[Tiếp tục NGÀY 4-6 →](02-4LAYER-DAYS4-6.md)**

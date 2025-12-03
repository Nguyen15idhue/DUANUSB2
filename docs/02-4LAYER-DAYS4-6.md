# 🚀 HỆ THỐNG USB DONGLE 4 LAYERS - NGÀY 4-6

## 📌 TIẾP TỤC TỪ NGÀY 3

Đã hoàn thành:
- ✅ NGÀY 1: Service Core & USB Detection
- ✅ NGÀY 2: Encryption & Machine Binding
- ✅ NGÀY 3: DLL Management

Còn lại:
- 🔄 NGÀY 4: IPC & Heartbeat Monitor
- 🔄 NGÀY 5: DLL Patch Project & Creator Tool
- 🔄 NGÀY 6: Testing & Installer

---

## 🗓️ NGÀY 4: IPC & HEARTBEAT MONITOR (8 giờ)

### ⏰ 09:00 - 11:00 | IPC Server (Named Pipes) (2 giờ)

#### **File: `Services/IPCServer.cs`**

```csharp
using System.IO.Pipes;
using System.Text;
using Serilog;
using DongleSyncService.Utils;

namespace DongleSyncService.Services
{
    public class IPCServer : IDisposable
    {
        private NamedPipeServerStream _pipeServer;
        private Thread _listenerThread;
        private bool _isRunning;
        private readonly StateManager _stateManager;

        public IPCServer(StateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public void Start()
        {
            try
            {
                Log.Information("Starting IPC Server...");
                _isRunning = true;
                _listenerThread = new Thread(ListenForConnections);
                _listenerThread.IsBackground = true;
                _listenerThread.Start();
                Log.Information("IPC Server started successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to start IPC Server");
                throw;
            }
        }

        public void Stop()
        {
            try
            {
                _isRunning = false;
                _pipeServer?.Close();
                _listenerThread?.Join(1000);
                Log.Information("IPC Server stopped");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error stopping IPC Server");
            }
        }

        private void ListenForConnections()
        {
            while (_isRunning)
            {
                try
                {
                    _pipeServer = new NamedPipeServerStream(
                        Constants.PipeName,
                        PipeDirection.InOut,
                        1,
                        PipeTransmissionMode.Message,
                        PipeOptions.Asynchronous
                    );

                    Log.Debug("IPC Server waiting for connection...");
                    _pipeServer.WaitForConnection();
                    Log.Debug("IPC Client connected");

                    HandleClient(_pipeServer);
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                    {
                        Log.Error(ex, "Error in IPC listener");
                        Thread.Sleep(1000); // Wait before retry
                    }
                }
                finally
                {
                    _pipeServer?.Dispose();
                }
            }
        }

        private void HandleClient(NamedPipeServerStream pipe)
        {
            try
            {
                // Read request
                var buffer = new byte[1024];
                var bytesRead = pipe.Read(buffer, 0, buffer.Length);
                var request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Log.Debug("IPC Request: {Request}", request);

                // Process request
                var response = ProcessRequest(request);

                // Send response
                var responseBytes = Encoding.UTF8.GetBytes(response);
                pipe.Write(responseBytes, 0, responseBytes.Length);
                pipe.Flush();

                Log.Debug("IPC Response sent: {Response}", response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling IPC client");
            }
        }

        private string ProcessRequest(string request)
        {
            try
            {
                var parts = request.Split('|');
                var command = parts[0];

                switch (command)
                {
                    case "CHECK_STATUS":
                        return GetStatusResponse();

                    case "VALIDATE_BINDING":
                        if (parts.Length > 1)
                        {
                            var usbGuid = parts[1];
                            return ValidateBindingResponse(usbGuid);
                        }
                        return "ERROR|Invalid request format";

                    case "PING":
                        return "PONG";

                    default:
                        return "ERROR|Unknown command";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing IPC request");
                return $"ERROR|{ex.Message}";
            }
        }

        private string GetStatusResponse()
        {
            var state = _stateManager.GetState();
            return $"STATUS|IsPatched={state.IsPatched}|UsbGuid={state.UsbGuid}";
        }

        private string ValidateBindingResponse(string usbGuid)
        {
            var state = _stateManager.GetState();
            
            if (!state.IsPatched)
            {
                return "INVALID|Not patched";
            }

            if (state.UsbGuid != usbGuid)
            {
                return "INVALID|USB GUID mismatch";
            }

            return "VALID";
        }

        public void Dispose()
        {
            Stop();
            _pipeServer?.Dispose();
        }
    }
}
```

**✅ Checkpoint**: IPC Server compile thành công

---

### ⏰ 11:00 - 12:00 | Dev Mode Support (1 giờ)

#### **File: `Models/DevModeConfig.cs`**

```csharp
using Newtonsoft.Json;

namespace DongleSyncService.Models
{
    public class DevModeConfig
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("allowedMachines")]
        public List<string> AllowedMachines { get; set; } = new List<string>();

        [JsonProperty("bypassBinding")]
        public bool BypassBinding { get; set; }

        [JsonProperty("bypassUSBValidation")]
        public bool BypassUSBValidation { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
```

#### **File: `Services/DevModeManager.cs`**

```csharp
using Newtonsoft.Json;
using Serilog;
using DongleSyncService.Models;
using DongleSyncService.Utils;

namespace DongleSyncService.Services
{
    public class DevModeManager
    {
        private readonly MachineBindingService _binding;
        private DevModeConfig _config;

        public DevModeManager(MachineBindingService binding)
        {
            _binding = binding;
            LoadConfig();
        }

        public bool IsDevModeEnabled()
        {
            if (_config == null || !_config.Enabled)
            {
                return false;
            }

            // Check if current machine is in allowed list
            var machineId = _binding.GetMachineFingerprint();
            return _config.AllowedMachines.Contains(machineId);
        }

        public bool ShouldBypassBinding()
        {
            return IsDevModeEnabled() && _config.BypassBinding;
        }

        public bool ShouldBypassUSBValidation()
        {
            return IsDevModeEnabled() && _config.BypassUSBValidation;
        }

        private void LoadConfig()
        {
            try
            {
                if (File.Exists(Constants.DevModeFile))
                {
                    var json = File.ReadAllText(Constants.DevModeFile);
                    _config = JsonConvert.DeserializeObject<DevModeConfig>(json);
                    Log.Information("Dev mode config loaded. Enabled: {Enabled}", _config?.Enabled);
                }
                else
                {
                    _config = new DevModeConfig { Enabled = false };
                    Log.Debug("Dev mode config not found. Dev mode disabled.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load dev mode config");
                _config = new DevModeConfig { Enabled = false };
            }
        }

        public void EnableDevMode(bool bypassBinding = true, bool bypassUSBValidation = false)
        {
            try
            {
                var machineId = _binding.GetMachineFingerprint();

                _config = new DevModeConfig
                {
                    Enabled = true,
                    AllowedMachines = new List<string> { machineId },
                    BypassBinding = bypassBinding,
                    BypassUSBValidation = bypassUSBValidation,
                    CreatedAt = DateTime.UtcNow
                };

                SaveConfig();
                Log.Information("Dev mode enabled for machine: {MachineId}", machineId.Substring(0, 16) + "...");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to enable dev mode");
            }
        }

        public void DisableDevMode()
        {
            try
            {
                _config = new DevModeConfig { Enabled = false };
                SaveConfig();
                Log.Information("Dev mode disabled");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to disable dev mode");
            }
        }

        private void SaveConfig()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_config, Formatting.Indented);
                File.WriteAllText(Constants.DevModeFile, json);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save dev mode config");
            }
        }
    }
}
```

**✅ Checkpoint**: Dev Mode compile thành công

---

### ⏰ 12:00 - 13:00 | 🍽️ NGHỈ TRƯA

---

### ⏰ 13:00 - 16:00 | Heartbeat Monitor (3 giờ)

#### **File: `Services/HeartbeatMonitor.cs`**

```csharp
using System.Timers;
using Serilog;
using DongleSyncService.Utils;
using Timer = System.Timers.Timer;

namespace DongleSyncService.Services
{
    public class HeartbeatMonitor : IDisposable
    {
        private Timer _timer;
        private readonly StateManager _stateManager;
        private readonly USBValidator _validator;
        private readonly DLLManager _dllManager;
        private bool _isRunning;

        public event EventHandler<HeartbeatEventArgs> HeartbeatFailed;

        public HeartbeatMonitor(
            StateManager stateManager,
            USBValidator validator,
            DLLManager dllManager)
        {
            _stateManager = stateManager;
            _validator = validator;
            _dllManager = dllManager;
        }

        public void Start()
        {
            try
            {
                Log.Information("Starting Heartbeat Monitor...");

                _timer = new Timer(Constants.HeartbeatInterval);
                _timer.Elapsed += OnHeartbeat;
                _timer.AutoReset = true;
                _timer.Start();

                _isRunning = true;
                Log.Information("Heartbeat Monitor started. Interval: {Interval}ms", Constants.HeartbeatInterval);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to start Heartbeat Monitor");
                throw;
            }
        }

        public void Stop()
        {
            try
            {
                _isRunning = false;
                _timer?.Stop();
                Log.Information("Heartbeat Monitor stopped");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error stopping Heartbeat Monitor");
            }
        }

        private void OnHeartbeat(object sender, ElapsedEventArgs e)
        {
            try
            {
                var state = _stateManager.GetState();

                // Only check if we're in patched state
                if (!state.IsPatched)
                {
                    return;
                }

                Log.Debug("Heartbeat check: Looking for USB {Guid}", state.UsbGuid);

                // Check if USB is still present
                var usbPresent = CheckUSBPresence(state.UsbGuid);

                if (!usbPresent)
                {
                    Log.Warning("Heartbeat FAILED: USB {Guid} not found!", state.UsbGuid);
                    HeartbeatFailed?.Invoke(this, new HeartbeatEventArgs(state.UsbGuid));
                }
                else
                {
                    Log.Debug("Heartbeat OK: USB {Guid} present", state.UsbGuid);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in heartbeat check");
            }
        }

        private bool CheckUSBPresence(string expectedGuid)
        {
            try
            {
                // Get all logical drives
                var drives = DriveInfo.GetDrives()
                    .Where(d => d.DriveType == DriveType.Removable && d.IsReady)
                    .ToList();

                foreach (var drive in drives)
                {
                    // Check if this drive has our dongle
                    if (_validator.ValidateDongle(drive.Name, out var config))
                    {
                        if (config.UsbGuid == expectedGuid)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error checking USB presence");
                return false;
            }
        }

        public void Dispose()
        {
            Stop();
            _timer?.Dispose();
        }
    }

    public class HeartbeatEventArgs : EventArgs
    {
        public string UsbGuid { get; }

        public HeartbeatEventArgs(string usbGuid)
        {
            UsbGuid = usbGuid;
        }
    }
}
```

**✅ Checkpoint**: Heartbeat Monitor compile thành công

---

### ⏰ 16:00 - 17:00 | Integration & Testing (1 giờ)

#### **Update: `DongleService.cs` - Add IPC & Heartbeat**

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
        private IPCServer _ipcServer;
        private HeartbeatMonitor _heartbeat;
        private DevModeManager _devMode;

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
                _devMode = new DevModeManager(_binding);
                _appFinder = new AppFinder();
                _dllManager = new DLLManager(_crypto, _appFinder);
                _validator = new USBValidator();
                
                // Start IPC Server
                _ipcServer = new IPCServer(_stateManager);
                _ipcServer.Start();
                
                // Start Heartbeat Monitor
                _heartbeat = new HeartbeatMonitor(_stateManager, _validator, _dllManager);
                _heartbeat.HeartbeatFailed += OnHeartbeatFailed;
                _heartbeat.Start();
                
                // Start USB Monitor
                _usbMonitor = new USBMonitor();
                _usbMonitor.USBInserted += OnUSBInserted;
                _usbMonitor.USBRemoved += OnUSBRemoved;
                _usbMonitor.Start();
                
                // Check if we're in a patched state without USB
                var state = _stateManager.GetState();
                if (state.IsPatched)
                {
                    Log.Warning("Service starting in patched state. Heartbeat will monitor USB presence.");
                }
                
                Log.Information("Dongle Service started successfully");
                Log.Information("Dev Mode: {DevMode}", _devMode.IsDevModeEnabled() ? "ENABLED" : "DISABLED");
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
                
                // Stop heartbeat
                if (_heartbeat != null)
                {
                    _heartbeat.HeartbeatFailed -= OnHeartbeatFailed;
                    _heartbeat.Stop();
                    _heartbeat.Dispose();
                }
                
                // Stop IPC
                _ipcServer?.Stop();
                _ipcServer?.Dispose();
                
                // Stop USB monitor
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

                Log.Information("Valid dongle detected! GUID: {Guid}", config.UsbGuid);

                // 2. Check or create binding
                if (!_devMode.ShouldBypassBinding())
                {
                    if (!File.Exists(Constants.BindKeyFile))
                    {
                        Log.Information("Creating new machine binding...");
                        _binding.CreateBinding(config.UsbGuid);
                    }
                    else if (!_binding.ValidateBinding(config.UsbGuid))
                    {
                        Log.Error("Binding validation failed!");
                        return;
                    }
                }
                else
                {
                    Log.Warning("Dev mode: Bypassing binding validation");
                }

                // 3. Patch DLL
                var donglePath = _validator.GetDonglePath(e.DriveName);
                var usbSerial = _validator.ComputeUSBHardwareKey(e.DriveName);

                if (_dllManager.PatchDLL(donglePath, usbSerial))
                {
                    // Update state
                    _stateManager.UpdateState(state =>
                    {
                        state.IsPatched = true;
                        state.UsbGuid = config.UsbGuid;
                        state.DllPath = _dllManager.GetPatchedDLLPath();
                        state.MachineId = _binding.GetMachineFingerprint();
                        state.LastPatchTime = DateTime.UtcNow;
                    });

                    Log.Information("✅ DLL PATCHED SUCCESSFULLY");
                }
                else
                {
                    Log.Error("❌ Failed to patch DLL");
                }

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

                // Only restore if we're in patched state
                if (!state.IsPatched)
                {
                    Log.Information("Not in patched state, no action needed");
                    return;
                }

                // Restore DLL
                if (!string.IsNullOrEmpty(state.DllPath))
                {
                    if (_dllManager.RestoreDLL(state.DllPath))
                    {
                        // Update state
                        _stateManager.UpdateState(s =>
                        {
                            s.IsPatched = false;
                            s.UsbGuid = null;
                            s.DllPath = null;
                            s.LastRestoreTime = DateTime.UtcNow;
                        });

                        // Delete binding
                        if (!_devMode.ShouldBypassBinding())
                        {
                            _binding.DeleteBinding();
                        }

                        Log.Information("✅ DLL RESTORED SUCCESSFULLY");
                    }
                    else
                    {
                        Log.Error("❌ Failed to restore DLL");
                    }
                }

                Log.Information("========================================");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing USB removal");
            }
        }

        private void OnHeartbeatFailed(object sender, HeartbeatEventArgs e)
        {
            try
            {
                Log.Warning("========================================");
                Log.Warning("HEARTBEAT FAILED - USB DISCONNECTED!");
                Log.Warning("Auto-restoring DLL...");
                Log.Warning("========================================");

                var state = _stateManager.GetState();

                if (!string.IsNullOrEmpty(state.DllPath))
                {
                    if (_dllManager.RestoreDLL(state.DllPath))
                    {
                        _stateManager.UpdateState(s =>
                        {
                            s.IsPatched = false;
                            s.UsbGuid = null;
                            s.DllPath = null;
                            s.LastRestoreTime = DateTime.UtcNow;
                        });

                        if (!_devMode.ShouldBypassBinding())
                        {
                            _binding.DeleteBinding();
                        }

                        Log.Information("✅ DLL AUTO-RESTORED SUCCESSFULLY");
                    }
                    else
                    {
                        Log.Error("❌ Failed to auto-restore DLL");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling heartbeat failure");
            }
        }
    }
}
```

#### **Test IPC & Heartbeat:**

```powershell
# Build
dotnet build

# Run service
dotnet run

# Expected logs:
# - IPC Server started
# - Heartbeat Monitor started (5s interval)
# - USB events trigger patching/restore
# - If USB removed without event → Heartbeat detects in 5s
```

**✅ Checkpoint NGÀY 4**:
- IPC Server hoạt động
- Dev Mode support
- Heartbeat Monitor hoạt động (detect USB removal in 5s)
- Layer 4 complete ✅

---

**[Tiếp tục NGÀY 5-6 →](03-4LAYER-DAYS5-6.md)**

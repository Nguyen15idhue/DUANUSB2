using Serilog;
using DongleSyncService.Services;
using DongleSyncService.Utils;

namespace DongleSyncService
{
    public class DongleService
    {
        private USBMonitor? _usbMonitor;
        private USBValidator? _validator;
        private StateManager? _stateManager;
        private CryptoService? _crypto;
        private MachineBindingService? _binding;
        private AppFinder? _appFinder;
        private DLLManager? _dllManager;
        private IPCServer? _ipcServer;
        private HeartbeatMonitor? _heartbeat;
        private DevModeManager? _devMode;

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
                
                // Check if CHC is installed by checking if DLL exists
                Log.Information("Checking if CHC Geomatics Office 2 is installed...");
                var dllPath = _appFinder.FindTargetDLL();
                if (string.IsNullOrEmpty(dllPath))
                {
                    Log.Error("CHC Geomatics Office 2 is not installed. Service cannot start.");
                    Log.Error("Please install CHC Geomatics Office 2 and run it at least once before using this service.");
                    return false;
                }
                Log.Information("CHC Geomatics Office 2 is installed. DLL found at: {Path}", dllPath);
                
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

                var (patchSuccess, dllHash, patchTimestamp) = _dllManager.PatchDLL(donglePath, usbSerial);
                
                if (patchSuccess)
                {
                    // Update state with security info
                    _stateManager.UpdateState(state =>
                    {
                        state.IsPatched = true;
                        state.UsbGuid = config.UsbGuid;
                        state.DllPath = _dllManager.GetPatchedDLLPath();
                        state.MachineId = _binding.GetMachineFingerprint();
                        state.LastPatchTime = DateTime.UtcNow;
                        
                        // SECURITY: Store hash and timestamp for integrity check
                        state.PatchedDllHash = dllHash;
                        state.PatchTimestamp = patchTimestamp;
                    });

                    // Show success notification
                    Utils.NotificationHelper.NotifyPatchSuccess();
                    
                    Log.Information("✅ DLL PATCHED SUCCESSFULLY");
                    Log.Information("🔒 Integrity monitoring enabled");
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

                // CRITICAL: Close app FIRST to ensure it reloads DLL
                if (!string.IsNullOrEmpty(state.DllPath))
                {
                    Log.Information("Closing app to ensure DLL reload...");
                    _dllManager.EnsureAppClosed(state.DllPath);
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
                            s.PatchedDllHash = null;
                            s.PatchTimestamp = null;
                            s.LastRestoreTime = DateTime.UtcNow;
                        });

                        // Delete binding
                        if (!_devMode.ShouldBypassBinding())
                        {
                            _binding.DeleteBinding();
                        }

                        // Show notification
                        Utils.NotificationHelper.NotifyUSBRemoved();

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

                // CRITICAL: Only restore if we're in patched state
                if (!state.IsPatched)
                {
                    Log.Information("Not in patched state, no action needed");
                    return;
                }

                // CRITICAL: Close app FIRST to force DLL reload on next launch
                if (!string.IsNullOrEmpty(state.DllPath))
                {
                    Log.Warning("Force closing app to ensure DLL reload...");
                    _dllManager.EnsureAppClosed(state.DllPath);
                    System.Threading.Thread.Sleep(1000); // Wait for process to fully exit
                }

                if (!string.IsNullOrEmpty(state.DllPath))
                {
                    if (_dllManager.RestoreDLL(state.DllPath))
                    {
                        _stateManager.UpdateState(s =>
                        {
                            s.IsPatched = false;
                            s.UsbGuid = null;
                            s.DllPath = null;
                            s.PatchedDllHash = null;
                            s.PatchTimestamp = null;
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
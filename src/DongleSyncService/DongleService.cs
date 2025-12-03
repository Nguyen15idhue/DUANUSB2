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
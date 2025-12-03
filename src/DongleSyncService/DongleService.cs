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
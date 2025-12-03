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
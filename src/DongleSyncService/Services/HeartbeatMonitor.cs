using System.Security.Cryptography;
using System.Timers;
using Serilog;
using DongleSyncService.Models;
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

                // SECURITY: Check DLL integrity first (detect manual replacement)
                if (!CheckDLLIntegrity(state))
                {
                    Log.Warning("⚠️ DLL INTEGRITY VIOLATION DETECTED!");
                    Log.Warning("User may have replaced the patched DLL - restoring original");
                    
                    // Immediately restore original DLL
                    if (!string.IsNullOrEmpty(state.DllPath))
                    {
                        _dllManager.RestoreDLL(state.DllPath);
                    }
                    
                    // Trigger heartbeat failure to force cleanup
                    HeartbeatFailed?.Invoke(this, new HeartbeatEventArgs(
                        state.UsbGuid,
                        "DLL integrity check failed - unauthorized modification detected"
                    ));
                    return;
                }

                // Check if USB is still present
                var usbPresent = CheckUSBPresence(state.UsbGuid);

                if (!usbPresent)
                {
                    Log.Warning("Heartbeat FAILED: USB {Guid} not found!", state.UsbGuid);
                    HeartbeatFailed?.Invoke(this, new HeartbeatEventArgs(state.UsbGuid, "USB disconnected"));
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

        /// <summary>
        /// Check if patched DLL has been tampered with (security check)
        /// </summary>
        private bool CheckDLLIntegrity(ServiceState state)
        {
            try
            {
                if (string.IsNullOrEmpty(state.DllPath) || !File.Exists(state.DllPath))
                {
                    Log.Warning("DLL file not found at expected path: {Path}", state.DllPath);
                    return false;
                }

                // Compute current hash
                using var sha256 = System.Security.Cryptography.SHA256.Create();
                using var stream = File.OpenRead(state.DllPath);
                var currentHash = sha256.ComputeHash(stream);
                var currentHashString = Convert.ToBase64String(currentHash);

                // Compare with stored hash (from when we patched it)
                if (!string.IsNullOrEmpty(state.PatchedDllHash))
                {
                    if (currentHashString != state.PatchedDllHash)
                    {
                        Log.Error("DLL integrity check FAILED!");
                        Log.Error("Expected hash: {Expected}", state.PatchedDllHash?.Substring(0, 16) + "...");
                        Log.Error("Current hash:  {Current}", currentHashString.Substring(0, 16) + "...");
                        Log.Error("⚠️ DLL has been modified after patching - possible security breach!");
                        return false;
                    }
                }

                // Check file modification time (should not be newer than patch time)
                var fileModifiedTime = File.GetLastWriteTimeUtc(state.DllPath);
                if (state.PatchTimestamp.HasValue)
                {
                    var timeDiff = fileModifiedTime - state.PatchTimestamp.Value;
                    if (timeDiff.TotalSeconds > 5) // Allow 5 seconds grace period
                    {
                        Log.Warning("DLL file modified AFTER patching!");
                        Log.Warning("Patch time: {PatchTime}", state.PatchTimestamp);
                        Log.Warning("File time:  {FileTime}", fileModifiedTime);
                        Log.Warning("⚠️ Possible manual file replacement detected!");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error checking DLL integrity");
                return false; // Fail-safe: treat error as integrity failure
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
        public string UsbGuid { get; set; }
        public string? Reason { get; set; }

        public HeartbeatEventArgs(string usbGuid, string? reason = null)
        {
            UsbGuid = usbGuid;
            Reason = reason;
        }
    }
}
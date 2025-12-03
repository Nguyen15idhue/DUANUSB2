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
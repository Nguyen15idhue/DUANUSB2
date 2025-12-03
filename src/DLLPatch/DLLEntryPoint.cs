using System.Runtime.InteropServices;
using Serilog;

namespace DLLPatch
{
    public static class DLLEntryPoint
    {
        private static bool _isInitialized = false;
        private static string _currentUsbGuid;

        // This will be called when DLL is loaded by CHC Geomatics Office 2
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public static void Initialize(string usbGuid)
        {
            if (_isInitialized)
            {
                return;
            }

            try
            {
                // Configure logging
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.File(
                        @"C:\ProgramData\DongleSyncService\logs\patch-.log",
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [PATCH] {Message:lj}{NewLine}{Exception}"
                    )
                    .CreateLogger();

                Log.Information("========================================");
                Log.Information("Patched DLL Initialized");
                Log.Information("USB GUID: {Guid}", usbGuid);
                Log.Information("========================================");

                _currentUsbGuid = usbGuid;

                // Validate binding
                if (!BindingValidator.ValidateLocalBinding(usbGuid))
                {
                    Log.Error("Binding validation FAILED!");
                    throw new UnauthorizedAccessException("Invalid machine binding");
                }

                Log.Information("Binding validation PASSED");

                // Start heartbeat check
                StartHeartbeatCheck();

                _isInitialized = true;
                Log.Information("Patched DLL ready");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to initialize patched DLL");
                throw;
            }
        }

        private static void StartHeartbeatCheck()
        {
            var timer = new System.Timers.Timer(10000); // 10 seconds
            timer.Elapsed += (s, e) =>
            {
                try
                {
                    if (!IPCClient.Ping())
                    {
                        Log.Warning("Heartbeat check failed - Service not responding");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Heartbeat check error");
                }
            };
            timer.Start();
        }

        // Extended functionality for CHC.CGO.Common.dll
        public static void EnableExtendedFeatures()
        {
            Log.Information("Extended features enabled");
            // Add custom features here
        }

        public static string GetLicenseInfo()
        {
            return $"Licensed to USB: {_currentUsbGuid}";
        }
    }
}
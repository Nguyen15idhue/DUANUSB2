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
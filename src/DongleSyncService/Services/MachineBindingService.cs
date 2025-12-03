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
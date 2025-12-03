using System.Management;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace DongleCreatorTool
{
    public class USBWriter
    {
        public static bool CreateDongle(
            string usbDrive,
            string patchDllPath,
            out string? errorMessage)
        {
            errorMessage = null;

            try
            {
                // 1. Verify USB drive
                if (!Directory.Exists(usbDrive))
                {
                    errorMessage = "USB drive not found";
                    return false;
                }

                // 2. Verify patch DLL exists
                if (!File.Exists(patchDllPath))
                {
                    errorMessage = "Patch DLL file not found";
                    return false;
                }

                // 3. Compute USB hardware key
                var usbKey = ComputeUSBHardwareKey(usbDrive);
                if (string.IsNullOrEmpty(usbKey))
                {
                    errorMessage = "Failed to compute USB hardware key";
                    return false;
                }

                // 4. Generate USB GUID
                var usbGuid = Guid.NewGuid().ToString();

                // 5. Create dongle folder
                var donglePath = Path.Combine(usbDrive, "dongle");
                Directory.CreateDirectory(donglePath);

                // 6. Create config.json
                var config = new
                {
                    usbGuid = usbGuid,
                    version = "1.0",
                    createdAt = DateTime.UtcNow
                };
                var configJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(Path.Combine(donglePath, "config.json"), configJson);

                // 7. Write dongle.key
                File.WriteAllText(Path.Combine(donglePath, "dongle.key"), usbKey);

                // 8. Encrypt and write patch DLL
                var dllData = File.ReadAllBytes(patchDllPath);
                var (encrypted, iv) = CryptoHelper.EncryptDLL(dllData, usbKey);

                File.WriteAllBytes(Path.Combine(donglePath, "patch.dll.enc"), encrypted);
                File.WriteAllBytes(Path.Combine(donglePath, "iv.bin"), iv);

                // 9. Create README.txt
                var readme = @"USB DONGLE FOR CHC GEOMATICS OFFICE 2
=====================================

This USB contains encrypted files for app enhancement.

Files:
- config.json: Dongle configuration
- dongle.key: Hardware authentication key
- patch.dll.enc: Encrypted patch file
- iv.bin: Encryption initialization vector

DO NOT:
- Copy these files to another USB
- Modify any files
- Delete any files

The system uses:
- Layer 1: USB Hardware ID validation
- Layer 2: AES-256 encryption
- Layer 3: Machine binding
- Layer 4: Runtime heartbeat monitoring

Created: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + @"
USB GUID: " + usbGuid;

                File.WriteAllText(Path.Combine(donglePath, "README.txt"), readme);

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error: {ex.Message}";
                return false;
            }
        }

        private static string ComputeUSBHardwareKey(string drivePath)
        {
            try
            {
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

                var deviceQuery = "SELECT * FROM Win32_DiskDrive WHERE Size IS NOT NULL";
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

                var combined = $"{volumeSerial}|{deviceId}";
                using var sha256 = SHA256.Create();
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                return Convert.ToBase64String(hashBytes);
            }
            catch (Exception ex)
            {
                // Log error and return empty string
                return string.Empty;
            }
        }
    }
}
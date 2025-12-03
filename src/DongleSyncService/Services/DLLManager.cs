using System.Security.Cryptography;
using System.Text;
using Serilog;
using DongleSyncService.Utils;

namespace DongleSyncService.Services
{
    public class DLLManager
    {
        private readonly CryptoService _crypto;
        private readonly AppFinder _appFinder;

        public DLLManager(CryptoService crypto, AppFinder appFinder)
        {
            _crypto = crypto;
            _appFinder = appFinder;
        }

        /// <summary>
        /// Backup original DLL
        /// </summary>
        public bool BackupDLL(string dllPath)
        {
            try
            {
                Log.Information("Backing up DLL: {Path}", dllPath);

                if (!File.Exists(dllPath))
                {
                    Log.Error("DLL file not found: {Path}", dllPath);
                    return false;
                }

                var backupPath = GetBackupPath(dllPath);
                
                // Don't backup if already exists (use existing backup)
                if (File.Exists(backupPath))
                {
                    Log.Information("Backup already exists: {Path}", backupPath);
                    return true;
                }

                // Copy file
                File.Copy(dllPath, backupPath, overwrite: false);

                // Save hash for verification
                var hash = ComputeFileHash(dllPath);
                File.WriteAllText(backupPath + ".hash", hash);

                Log.Information("Backup created successfully: {Path}", backupPath);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to backup DLL");
                return false;
            }
        }

        /// <summary>
        /// Restore original DLL from backup
        /// </summary>
        public bool RestoreDLL(string dllPath)
        {
            try
            {
                Log.Information("Restoring DLL: {Path}", dllPath);

                var backupPath = GetBackupPath(dllPath);
                
                if (!File.Exists(backupPath))
                {
                    Log.Error("Backup file not found: {Path}", backupPath);
                    return false;
                }

                // Verify backup integrity
                if (File.Exists(backupPath + ".hash"))
                {
                    var expectedHash = File.ReadAllText(backupPath + ".hash");
                    var actualHash = ComputeFileHash(backupPath);
                    
                    if (expectedHash != actualHash)
                    {
                        Log.Error("Backup file corrupted (hash mismatch)");
                        return false;
                    }
                }

                // Restore file
                File.Copy(backupPath, dllPath, overwrite: true);

                Log.Information("DLL restored successfully");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to restore DLL");
                return false;
            }
        }

        private string GetBackupPath(string dllPath)
        {
            var fileName = Path.GetFileName(dllPath);
            return Path.Combine(Constants.BackupFolder, fileName + ".bak");
        }

        private string ComputeFileHash(string filePath)
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hash = sha256.ComputeHash(stream);
            return Convert.ToBase64String(hash);
        }
        /// <summary>
        /// Patch DLL with encrypted version from USB
        /// </summary>
        public bool PatchDLL(string donglePath, string usbSerial)
        {
            try
            {
                Log.Information("Patching DLL from dongle: {Path}", donglePath);

                // 1. Find target DLL
                var dllPath = _appFinder.FindTargetDLL();
                if (string.IsNullOrEmpty(dllPath))
                {
                    Log.Error("Target DLL not found");
                    return false;
                }

                // 2. Check if App X is running
                if (IsDLLInUse(dllPath))
                {
                    Log.Warning("DLL is currently in use. Please close App X first.");
                    return false;
                }

                // 3. Backup original DLL
                if (!BackupDLL(dllPath))
                {
                    Log.Error("Failed to backup DLL");
                    return false;
                }

                // 4. Decrypt patch DLL from USB
                var encPath = Path.Combine(donglePath, Constants.PatchDllFile);
                var ivPath = Path.Combine(donglePath, Constants.PatchIvFile);

                if (!File.Exists(encPath) || !File.Exists(ivPath))
                {
                    Log.Error("Patch files not found in dongle");
                    return false;
                }

                var decryptedDll = _crypto.DecryptDLL(encPath, ivPath, usbSerial);

                // 5. Write patched DLL
                File.WriteAllBytes(dllPath, decryptedDll);

                Log.Information("DLL patched successfully: {Path}", dllPath);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to patch DLL");
                
                // Try to restore backup on error
                try
                {
                    var dllPath = _appFinder.FindTargetDLL();
                    if (!string.IsNullOrEmpty(dllPath))
                    {
                        RestoreDLL(dllPath);
                    }
                }
                catch { }
                
                return false;
            }
        }

        /// <summary>
        /// Check if DLL is currently loaded by any process
        /// </summary>
        private bool IsDLLInUse(string dllPath)
        {
            try
            {
                // Try to open file with exclusive access
                using var fs = File.Open(dllPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return false;
            }
            catch (IOException)
            {
                // File is locked = in use
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get path of currently patched DLL
        /// </summary>
        public string GetPatchedDLLPath()
        {
            return _appFinder.FindTargetDLL();
        }
    }
}
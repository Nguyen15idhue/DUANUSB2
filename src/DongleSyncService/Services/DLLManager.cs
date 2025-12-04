using System.Diagnostics;
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
        
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 2000;

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
        /// Returns tuple: (success, dllHash, timestamp)
        /// </summary>
        public (bool success, string? dllHash, DateTime? timestamp) PatchDLL(string donglePath, string usbSerial)
        {
            try
            {
                Log.Information("Patching DLL from dongle: {Path}", donglePath);

                // 1. Find target DLL
                var dllPath = _appFinder.FindTargetDLL();
                if (string.IsNullOrEmpty(dllPath))
                {
                    Log.Error("Target DLL not found");
                    return (false, null, null);
                }

                // 2. Close app if running (with retry mechanism)
                if (!EnsureAppClosed(dllPath))
                {
                    Log.Error("Failed to close application - DLL is still in use");
                    return (false, null, null);
                }

                // 3. Backup original DLL
                if (!BackupDLL(dllPath))
                {
                    Log.Error("Failed to backup DLL");
                    return (false, null, null);
                }

                // 4. Decrypt patch DLL from USB
                var encPath = Path.Combine(donglePath, Constants.PatchDllFile);
                var ivPath = Path.Combine(donglePath, Constants.PatchIvFile);

                if (!File.Exists(encPath) || !File.Exists(ivPath))
                {
                    Log.Error("Patch files not found in dongle");
                    return (false, null, null);
                }

                var decryptedDll = _crypto.DecryptDLL(encPath, ivPath, usbSerial);

                // 5. Write patched DLL (with retry)
                for (int attempt = 1; attempt <= MaxRetries; attempt++)
                {
                    try
                    {
                        File.WriteAllBytes(dllPath, decryptedDll);
                        Log.Information("DLL patched successfully: {Path}", dllPath);
                        
                        // SECURITY: Compute hash of patched DLL for integrity check
                        var patchedHash = ComputeFileHash(dllPath);
                        var patchTimestamp = DateTime.UtcNow;
                        
                        Log.Information("📊 DLL Hash (for integrity check): {Hash}", 
                            patchedHash.Substring(0, 16) + "...");
                        
                        return (true, patchedHash, patchTimestamp);
                    }
                    catch (IOException ex) when (attempt < MaxRetries)
                    {
                        Log.Warning("Failed to write DLL (attempt {Attempt}/{Max}): {Error}", 
                            attempt, MaxRetries, ex.Message);
                        Thread.Sleep(RetryDelayMs);
                    }
                }

                Log.Error("Failed to write DLL after {Max} attempts", MaxRetries);
                return (false, null, null);
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
                
                return (false, null, null);
            }
        }

        /// <summary>
        /// Ensure app is closed before patching DLL
        /// Tries graceful close first, then force kill if needed
        /// </summary>
        private bool EnsureAppClosed(string dllPath)
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                // Check if DLL is in use
                if (!IsDLLInUse(dllPath))
                {
                    Log.Information("DLL is not in use - ready to patch");
                    return true;
                }

                Log.Warning("DLL is in use (attempt {Attempt}/{Max}) - attempting to close app...", 
                    attempt, MaxRetries);

                // Find and close processes using the DLL
                var processes = FindProcessesUsingDLL(dllPath);
                if (processes.Count > 0)
                {
                    Log.Information("Found {Count} processes using DLL", processes.Count);
                    
                    foreach (var proc in processes)
                    {
                        try
                        {
                            var procName = proc.ProcessName;
                            Log.Information("Closing process: {Name} (PID: {Id})", 
                                procName, proc.Id);

                            // Show notification to user
                            if (attempt == 1)
                            {
                                NotificationHelper.NotifyAppClosed(procName);
                            }

                            // Try graceful close first
                            if (attempt < MaxRetries)
                            {
                                proc.CloseMainWindow();
                                proc.WaitForExit(3000); // Wait 3 seconds
                            }
                            else
                            {
                                // Force kill on last attempt
                                Log.Warning("Force killing process: {Name} (PID: {Id})", 
                                    procName, proc.Id);
                                proc.Kill();
                                proc.WaitForExit(2000);
                            }

                            Log.Information("Process closed successfully: {Name}", procName);
                        }
                        catch (Exception ex)
                        {
                            Log.Warning(ex, "Failed to close process: {Name}", proc.ProcessName);
                        }
                        finally
                        {
                            proc.Dispose();
                        }
                    }
                }

                // Wait before retry
                if (attempt < MaxRetries)
                {
                    Thread.Sleep(RetryDelayMs);
                }
            }

            // Final check
            return !IsDLLInUse(dllPath);
        }

        /// <summary>
        /// Find all processes that are using the specified DLL
        /// </summary>
        private List<Process> FindProcessesUsingDLL(string dllPath)
        {
            var result = new List<Process>();
            var dllFileName = Path.GetFileName(dllPath).ToLowerInvariant();
            var dllDirectory = Path.GetDirectoryName(dllPath)?.ToLowerInvariant() ?? "";

            try
            {
                // Search for processes by known app names
                var appProcessNames = new[] 
                { 
                    "CHC", 
                    "Geomatics", 
                    "CGO",
                    "CHCNAV"
                };

                var allProcesses = Process.GetProcesses();
                
                foreach (var proc in allProcesses)
                {
                    try
                    {
                        // Check if process name matches known app names
                        var isKnownApp = appProcessNames.Any(name => 
                            proc.ProcessName.Contains(name, StringComparison.OrdinalIgnoreCase));

                        if (isKnownApp)
                        {
                            result.Add(proc);
                            Log.Debug("Found known app process: {Name} (PID: {Id})", 
                                proc.ProcessName, proc.Id);
                            continue;
                        }

                        // Check if process is using the DLL
                        if (proc.Modules != null)
                        {
                            foreach (ProcessModule module in proc.Modules)
                            {
                                if (module.FileName.Equals(dllPath, StringComparison.OrdinalIgnoreCase))
                                {
                                    result.Add(proc);
                                    Log.Debug("Found process using DLL: {Name} (PID: {Id})", 
                                        proc.ProcessName, proc.Id);
                                    break;
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Ignore access denied for system processes
                        proc.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error finding processes using DLL");
            }

            return result;
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
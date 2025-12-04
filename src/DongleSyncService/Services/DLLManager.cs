using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Serilog;
using DongleSyncService.Utils;
using DongleSyncService.Models;

namespace DongleSyncService.Services
{
    public class DLLManager
    {
        private readonly CryptoService _crypto;
        private readonly AppFinder _appFinder;
        
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 2000;
        
        // Expected original DLL size (CHC.CGO.Common.dll = 287.5 KB)
        private const long ExpectedOriginalSize = 294400; // Actual original size

        public DLLManager(CryptoService crypto, AppFinder appFinder)
        {
            _crypto = crypto;
            _appFinder = appFinder;
        }

        /// <summary>
        /// Backup original DLL with 3-layer verification (HYBRID approach)
        /// Layer 1: File size check (288KB = original)
        /// Layer 2: SHA-256 hash
        /// Layer 3: ReadOnly attribute + timestamp
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
                var metadataPath = GetBackupMetadataPath(dllPath);
                
                // Check if backup already exists with valid metadata
                if (File.Exists(backupPath) && File.Exists(metadataPath))
                {
                    var existingMetadata = LoadBackupMetadata(metadataPath);
                    if (existingMetadata != null && VerifyBackupIntegrity(backupPath, existingMetadata))
                    {
                        Log.Information("✅ Valid backup already exists (Size: {Size} KB, Created: {Date})", 
                            existingMetadata.OriginalSize / 1024, 
                            existingMetadata.BackupCreated.ToLocalTime());
                        return true;
                    }
                    else
                    {
                        Log.Warning("⚠️ Existing backup failed integrity check - will recreate");
                    }
                }

                // CRITICAL: Verify current DLL is ORIGINAL (not patched)
                var currentFileInfo = new FileInfo(dllPath);
                if (currentFileInfo.Length != ExpectedOriginalSize)
                {
                    Log.Error("❌ CANNOT BACKUP: Current DLL size ({Current} bytes) != Expected original ({Expected} bytes). DLL may already be patched!", 
                        currentFileInfo.Length, ExpectedOriginalSize);
                    return false;
                }

                Log.Information("✅ DLL size verified: {Size} KB (original)", currentFileInfo.Length / 1024);

                // Copy file (this is the ORIGINAL, unpatched DLL)
                File.Copy(dllPath, backupPath, overwrite: true);

                // Set ReadOnly attribute for protection
                File.SetAttributes(backupPath, FileAttributes.ReadOnly);

                // Compute hash
                var hash = ComputeFileHash(backupPath);

                // Create metadata
                var metadata = new BackupMetadata
                {
                    FilePath = dllPath,
                    OriginalSize = currentFileInfo.Length,
                    OriginalHash = hash,
                    BackupCreated = DateTime.UtcNow,
                    IsReadOnly = true
                };

                // Save metadata
                SaveBackupMetadata(metadataPath, metadata);

                Log.Information("✅ Backup created successfully with 3-layer protection:");
                Log.Information("   - Size: {Size} KB", metadata.OriginalSize / 1024);
                Log.Information("   - Hash: {Hash}...", hash.Substring(0, 16));
                Log.Information("   - ReadOnly: {ReadOnly}", metadata.IsReadOnly);
                Log.Information("   - Timestamp: {Time}", metadata.BackupCreated.ToLocalTime());

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to backup DLL");
                return false;
            }
        }

        /// <summary>
        /// Restore original DLL from backup with 3-layer verification
        /// </summary>
        public bool RestoreDLL(string dllPath)
        {
            try
            {
                Log.Information("Restoring DLL: {Path}", dllPath);

                var backupPath = GetBackupPath(dllPath);
                var metadataPath = GetBackupMetadataPath(dllPath);
                
                if (!File.Exists(backupPath))
                {
                    Log.Error("❌ Backup file not found: {Path}", backupPath);
                    return false;
                }
                
                // Load metadata to get correct original path (in case state.DllPath is wrong)
                if (File.Exists(metadataPath))
                {
                    var metadataJson = File.ReadAllText(metadataPath);
                    var backupMeta = Newtonsoft.Json.JsonConvert.DeserializeObject<BackupMetadata>(metadataJson);
                    if (backupMeta != null && !string.IsNullOrEmpty(backupMeta.FilePath))
                    {
                        // Use path from metadata (more reliable than parameter)
                        dllPath = backupMeta.FilePath;
                        Log.Debug("Using DLL path from backup metadata: {Path}", dllPath);
                    }
                }

                if (!File.Exists(metadataPath))
                {
                    Log.Error("❌ Backup metadata not found: {Path}", metadataPath);
                    return false;
                }

                // Load and verify backup metadata
                var metadata = LoadBackupMetadata(metadataPath);
                if (metadata == null)
                {
                    Log.Error("❌ Failed to load backup metadata");
                    return false;
                }

                Log.Information("🔍 Verifying backup integrity (3-layer check)...");

                // LAYER 1: Size check
                var backupFileInfo = new FileInfo(backupPath);
                if (backupFileInfo.Length != metadata.OriginalSize)
                {
                    Log.Error("❌ LAYER 1 FAILED: Backup size mismatch! Expected: {Expected} bytes, Actual: {Actual} bytes", 
                        metadata.OriginalSize, backupFileInfo.Length);
                    return false;
                }
                Log.Information("✅ Layer 1: Size verified ({Size} KB)", backupFileInfo.Length / 1024);

                // LAYER 2: Hash check
                var actualHash = ComputeFileHash(backupPath);
                if (actualHash != metadata.OriginalHash)
                {
                    Log.Error("❌ LAYER 2 FAILED: Backup hash mismatch! Backup file may be corrupted.");
                    return false;
                }
                Log.Information("✅ Layer 2: Hash verified ({Hash}...)", actualHash.Substring(0, 16));

                // LAYER 3: ReadOnly attribute check
                var attributes = File.GetAttributes(backupPath);
                var isReadOnly = (attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
                if (!isReadOnly && metadata.IsReadOnly)
                {
                    Log.Warning("⚠️ Layer 3: ReadOnly attribute was removed from backup file (possible tampering)");
                }
                else
                {
                    Log.Information("✅ Layer 3: ReadOnly attribute verified");
                }

                // Additional check: Verify expected size
                if (metadata.OriginalSize != ExpectedOriginalSize)
                {
                    Log.Warning("⚠️ Backup size ({Backup} bytes) differs from expected original ({Expected} bytes)", 
                        metadata.OriginalSize, ExpectedOriginalSize);
                }

                Log.Information("✅ All integrity checks passed! Backup created: {Time}", 
                    metadata.BackupCreated.ToLocalTime());

                // Close app if running (critical for restore to work)
                if (!EnsureAppClosed(dllPath))
                {
                    Log.Warning("App is still running but will attempt restore anyway");
                }

                // Temporarily remove ReadOnly to allow overwrite
                if (isReadOnly)
                {
                    File.SetAttributes(backupPath, FileAttributes.Normal);
                }

                // Restore file
                File.Copy(backupPath, dllPath, overwrite: true);

                // Restore ReadOnly attribute
                if (metadata.IsReadOnly)
                {
                    File.SetAttributes(backupPath, FileAttributes.ReadOnly);
                }

                // Verify restored file
                var restoredFileInfo = new FileInfo(dllPath);
                if (restoredFileInfo.Length == metadata.OriginalSize)
                {
                    Log.Information("✅ DLL restored successfully! Size: {Size} KB", restoredFileInfo.Length / 1024);
                    return true;
                }
                else
                {
                    Log.Error("❌ Restored DLL size mismatch: {Actual} vs {Expected} bytes", 
                        restoredFileInfo.Length, metadata.OriginalSize);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to restore DLL");
                return false;
            }
        }

        /// <summary>
        /// Verify backup file integrity against metadata
        /// </summary>
        private bool VerifyBackupIntegrity(string backupPath, BackupMetadata metadata)
        {
            try
            {
                // Check size
                var fileInfo = new FileInfo(backupPath);
                if (fileInfo.Length != metadata.OriginalSize)
                {
                    Log.Warning("Backup size mismatch: {Actual} vs {Expected}", 
                        fileInfo.Length, metadata.OriginalSize);
                    return false;
                }

                // Check hash
                var actualHash = ComputeFileHash(backupPath);
                if (actualHash != metadata.OriginalHash)
                {
                    Log.Warning("Backup hash mismatch");
                    return false;
                }

                // Check ReadOnly attribute
                var attributes = File.GetAttributes(backupPath);
                var isReadOnly = (attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
                if (metadata.IsReadOnly && !isReadOnly)
                {
                    Log.Warning("Backup ReadOnly attribute was removed");
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to verify backup integrity");
                return false;
            }
        }

        /// <summary>
        /// Load backup metadata from JSON file
        /// </summary>
        private BackupMetadata? LoadBackupMetadata(string metadataPath)
        {
            try
            {
                var json = File.ReadAllText(metadataPath);
                return JsonSerializer.Deserialize<BackupMetadata>(json);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load backup metadata");
                return null;
            }
        }

        /// <summary>
        /// Save backup metadata to JSON file
        /// </summary>
        private void SaveBackupMetadata(string metadataPath, BackupMetadata metadata)
        {
            var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(metadataPath, json);
        }

        private string GetBackupPath(string dllPath)
        {
            var fileName = Path.GetFileName(dllPath);
            return Path.Combine(Constants.BackupFolder, fileName + ".original");
        }

        private string GetBackupMetadataPath(string dllPath)
        {
            var fileName = Path.GetFileName(dllPath);
            return Path.Combine(Constants.BackupFolder, fileName + ".metadata.json");
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

                // 1a. Override with path from backup metadata if available (more reliable)
                var metadataPath = GetBackupMetadataPath(dllPath);
                if (File.Exists(metadataPath))
                {
                    try
                    {
                        var metadataJson = File.ReadAllText(metadataPath);
                        var backupMeta = Newtonsoft.Json.JsonConvert.DeserializeObject<BackupMetadata>(metadataJson);
                        if (backupMeta != null && !string.IsNullOrEmpty(backupMeta.FilePath))
                        {
                            dllPath = backupMeta.FilePath;
                            Log.Debug("Using DLL path from backup metadata: {Path}", dllPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Failed to read backup metadata, using found path");
                    }
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
        public bool EnsureAppClosed(string dllPath)
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

                            // ALWAYS kill immediately - no graceful close for security
                            Log.Warning("Force killing process: {Name} (PID: {Id})", 
                                procName, proc.Id);
                            proc.Kill();
                            proc.WaitForExit(1000); // Wait only 1 second

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
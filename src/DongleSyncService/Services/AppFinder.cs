using System.Text.RegularExpressions;
using Serilog;
using DongleSyncService.Utils;

namespace DongleSyncService.Services
{
    public class AppFinder
    {
        /// <summary>
        /// Find CHC.CGO.Common.dll in system
        /// </summary>
        public string FindTargetDLL()
        {
            try
            {
                Log.Information("Searching for target DLL: {DLL}", Constants.TargetDllName);

                // 1. Check cache first
                var cachedPath = GetCachedPath();
                if (!string.IsNullOrEmpty(cachedPath) && File.Exists(cachedPath))
                {
                    Log.Information("Using cached DLL path: {Path}", cachedPath);
                    return cachedPath;
                }

                // 2. Search common locations
                var searchPaths = new[]
                {
                    @"C:\Program Files",
                    @"C:\Program Files (x86)",
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), // AppData\Roaming
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), // AppData\Local
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CHCNAV") // Direct path
                }.Distinct().Where(p => !string.IsNullOrEmpty(p));

                foreach (var basePath in searchPaths.Where(Directory.Exists))
                {
                    Log.Debug("Searching in: {Path}", basePath);
                    
                    var foundPath = SearchDirectory(basePath, Constants.TargetDllName, maxDepth: 4);
                    if (!string.IsNullOrEmpty(foundPath))
                    {
                        CachePath(foundPath);
                        Log.Information("DLL found: {Path}", foundPath);
                        return foundPath;
                    }
                }

                Log.Warning("Target DLL not found");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error finding target DLL");
                return null;
            }
        }

        private string SearchDirectory(string path, string fileName, int maxDepth, int currentDepth = 0)
        {
            if (currentDepth > maxDepth) return null;

            try
            {
                // Search files in current directory
                var files = Directory.GetFiles(path, fileName, SearchOption.TopDirectoryOnly);
                if (files.Length > 0)
                {
                    // Filter out system/temp folders
                    var validFile = files.FirstOrDefault(f => IsValidPath(f));
                    if (validFile != null)
                        return validFile;
                }

                // Search subdirectories
                var directories = Directory.GetDirectories(path);
                foreach (var dir in directories)
                {
                    if (ShouldSkipDirectory(dir)) continue;

                    var result = SearchDirectory(dir, fileName, maxDepth, currentDepth + 1);
                    if (result != null) return result;
                }
            }
            catch
            {
                // Ignore access denied errors
            }

            return null;
        }

        private bool IsValidPath(string path)
        {
            var lowerPath = path.ToLowerInvariant();
            
            // Exclude system folders
            var excludePatterns = new[]
            {
                "\\windows\\",
                "\\winsxs\\",
                "\\temp\\",
                "\\cache\\",
                "\\backup\\",
                "\\$"
            };

            return !excludePatterns.Any(pattern => lowerPath.Contains(pattern));
        }

        private bool ShouldSkipDirectory(string directory)
        {
            var name = Path.GetFileName(directory).ToLowerInvariant();
            
            var skipList = new[]
            {
                "windows", "winsxs", "temp", "cache", 
                "$recycle.bin", "system volume information"
            };

            return skipList.Contains(name);
        }

        private string GetCachedPath()
        {
            try
            {
                var cacheFile = Path.Combine(Constants.ProgramData, "app_cache.txt");
                if (File.Exists(cacheFile))
                {
                    return File.ReadAllText(cacheFile).Trim();
                }
            }
            catch { }
            return null;
        }

        private void CachePath(string path)
        {
            try
            {
                var cacheFile = Path.Combine(Constants.ProgramData, "app_cache.txt");
                File.WriteAllText(cacheFile, path);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to cache DLL path");
            }
        }
    }
}
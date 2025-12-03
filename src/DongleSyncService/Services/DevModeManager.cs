using Newtonsoft.Json;
using Serilog;
using DongleSyncService.Models;
using DongleSyncService.Utils;

namespace DongleSyncService.Services
{
    public class DevModeManager
    {
        private readonly MachineBindingService _binding;
        private DevModeConfig _config;

        public DevModeManager(MachineBindingService binding)
        {
            _binding = binding;
            LoadConfig();
        }

        public bool IsDevModeEnabled()
        {
            if (_config == null || !_config.Enabled)
            {
                return false;
            }

            // Check if current machine is in allowed list
            var machineId = _binding.GetMachineFingerprint();
            return _config.AllowedMachines.Contains(machineId);
        }

        public bool ShouldBypassBinding()
        {
            return IsDevModeEnabled() && _config.BypassBinding;
        }

        public bool ShouldBypassUSBValidation()
        {
            return IsDevModeEnabled() && _config.BypassUSBValidation;
        }

        private void LoadConfig()
        {
            try
            {
                if (File.Exists(Constants.DevModeFile))
                {
                    var json = File.ReadAllText(Constants.DevModeFile);
                    _config = JsonConvert.DeserializeObject<DevModeConfig>(json);
                    Log.Information("Dev mode config loaded. Enabled: {Enabled}", _config?.Enabled);
                }
                else
                {
                    _config = new DevModeConfig { Enabled = false };
                    Log.Debug("Dev mode config not found. Dev mode disabled.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load dev mode config");
                _config = new DevModeConfig { Enabled = false };
            }
        }

        public void EnableDevMode(bool bypassBinding = true, bool bypassUSBValidation = false)
        {
            try
            {
                var machineId = _binding.GetMachineFingerprint();

                _config = new DevModeConfig
                {
                    Enabled = true,
                    AllowedMachines = new List<string> { machineId },
                    BypassBinding = bypassBinding,
                    BypassUSBValidation = bypassUSBValidation,
                    CreatedAt = DateTime.UtcNow
                };

                SaveConfig();
                Log.Information("Dev mode enabled for machine: {MachineId}", machineId.Substring(0, 16) + "...");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to enable dev mode");
            }
        }

        public void DisableDevMode()
        {
            try
            {
                _config = new DevModeConfig { Enabled = false };
                SaveConfig();
                Log.Information("Dev mode disabled");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to disable dev mode");
            }
        }

        private void SaveConfig()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_config, Formatting.Indented);
                File.WriteAllText(Constants.DevModeFile, json);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save dev mode config");
            }
        }
    }
}
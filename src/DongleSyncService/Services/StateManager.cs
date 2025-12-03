using Newtonsoft.Json;
using Serilog;
using DongleSyncService.Models;
using DongleSyncService.Utils;

namespace DongleSyncService.Services
{
    public class StateManager
    {
        private ServiceState _state;
        private readonly object _lock = new object();

        public StateManager()
        {
            EnsureDirectories();
            LoadState();
        }

        private void EnsureDirectories()
        {
            Directory.CreateDirectory(Constants.ProgramData);
            Directory.CreateDirectory(Constants.LogFolder);
            Directory.CreateDirectory(Constants.BackupFolder);
        }

        public ServiceState GetState()
        {
            lock (_lock)
            {
                return _state;
            }
        }

        public void UpdateState(Action<ServiceState> updateAction)
        {
            lock (_lock)
            {
                updateAction(_state);
                SaveState();
            }
        }

        private void LoadState()
        {
            try
            {
                if (File.Exists(Constants.StateFile))
                {
                    var json = File.ReadAllText(Constants.StateFile);
                    _state = JsonConvert.DeserializeObject<ServiceState>(json);
                    Log.Information("State loaded successfully");
                }
                else
                {
                    _state = new ServiceState();
                    SaveState();
                    Log.Information("New state created");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load state, creating new");
                _state = new ServiceState();
            }
        }

        private void SaveState()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_state, Formatting.Indented);
                File.WriteAllText(Constants.StateFile, json);
                Log.Debug("State saved successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save state");
            }
        }
    }
}
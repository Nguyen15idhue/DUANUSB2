namespace DongleSyncService.Utils
{
    public static class Constants
    {
        // Service
        public const string ServiceName = "DongleSyncService";
        public const string ServiceDisplayName = "USB Dongle Sync Service";
        
        // Paths
        public const string ProgramData = @"C:\ProgramData\DongleSyncService";
        public const string LogFolder = @"C:\ProgramData\DongleSyncService\logs";
        public const string ConfigFile = @"C:\ProgramData\DongleSyncService\config.json";
        public const string StateFile = @"C:\ProgramData\DongleSyncService\state.json";
        public const string BindKeyFile = @"C:\ProgramData\DongleSyncService\bind.key";
        public const string DevModeFile = @"C:\ProgramData\DongleSyncService\devmode.json";
        public const string BackupFolder = @"C:\ProgramData\DongleSyncService\backups";
        
        // USB Dongle
        public const string DongleFolderName = "dongle";
        public const string DongleConfigFile = "config.json";
        public const string DongleKeyFile = "dongle.key";
        public const string PatchDllFile = "patch.dll.enc";
        public const string PatchIvFile = "iv.bin";
        
        // Target
        public const string TargetDllName = "CHC.CGO.Common.dll";
        
        // IPC
        public const string PipeName = "DongleSyncPipe";
        
        // Timing
        public const int HeartbeatInterval = 5000; // 5 seconds
        public const int IPCTimeout = 10000; // 10 seconds
    }
}
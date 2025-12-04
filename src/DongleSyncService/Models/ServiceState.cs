using Newtonsoft.Json;

namespace DongleSyncService.Models
{
    public class ServiceState
    {
        [JsonProperty("isPatched")]
        public bool IsPatched { get; set; }
        
        [JsonProperty("usbGuid")]
        public string UsbGuid { get; set; }
        
        [JsonProperty("dllPath")]
        public string DllPath { get; set; }
        
        [JsonProperty("machineId")]
        public string MachineId { get; set; }
        
        [JsonProperty("lastPatchTime")]
        public DateTime? LastPatchTime { get; set; }
        
        [JsonProperty("lastRestoreTime")]
        public DateTime? LastRestoreTime { get; set; }
        
        // Security: Store hash of patched DLL to detect tampering
        [JsonProperty("patchedDllHash")]
        public string? PatchedDllHash { get; set; }
        
        // Security: Store exact patch timestamp to detect file replacement
        [JsonProperty("patchTimestamp")]
        public DateTime? PatchTimestamp { get; set; }
    }
}
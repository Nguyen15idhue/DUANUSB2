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
    }
}
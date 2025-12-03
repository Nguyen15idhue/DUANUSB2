using Newtonsoft.Json;

namespace DongleSyncService.Models
{
    public class BindingData
    {
        [JsonProperty("usbGuid")]
        public string UsbGuid { get; set; }
        
        [JsonProperty("machineFingerprint")]
        public string MachineFingerprint { get; set; }
        
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
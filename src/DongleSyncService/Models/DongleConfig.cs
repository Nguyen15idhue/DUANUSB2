using Newtonsoft.Json;

namespace DongleSyncService.Models
{
    public class DongleConfig
    {
        [JsonProperty("usbGuid")]
        public string UsbGuid { get; set; }
        
        [JsonProperty("version")]
        public string Version { get; set; }
        
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
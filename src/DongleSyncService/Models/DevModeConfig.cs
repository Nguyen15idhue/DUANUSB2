using Newtonsoft.Json;

namespace DongleSyncService.Models
{
    public class DevModeConfig
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("allowedMachines")]
        public List<string> AllowedMachines { get; set; } = new List<string>();

        [JsonProperty("bypassBinding")]
        public bool BypassBinding { get; set; }

        [JsonProperty("bypassUSBValidation")]
        public bool BypassUSBValidation { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
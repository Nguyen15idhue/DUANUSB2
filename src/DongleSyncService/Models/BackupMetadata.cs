using System.Text.Json.Serialization;

namespace DongleSyncService.Models
{
    /// <summary>
    /// Metadata for DLL backup verification (3-layer security)
    /// </summary>
    public class BackupMetadata
    {
        [JsonPropertyName("filePath")]
        public string FilePath { get; set; } = string.Empty;

        [JsonPropertyName("originalSize")]
        public long OriginalSize { get; set; }

        [JsonPropertyName("originalHash")]
        public string OriginalHash { get; set; } = string.Empty;

        [JsonPropertyName("backupCreated")]
        public DateTime BackupCreated { get; set; }

        [JsonPropertyName("isReadOnly")]
        public bool IsReadOnly { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; } = "1.0";
    }
}

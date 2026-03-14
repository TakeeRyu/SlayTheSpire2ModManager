using System.Text.Json.Serialization;

namespace SlayTheSpire2ModManager.Infrastructure.Models
{
    public class ModMetadata
    {
        public string? Name { get; set; }

        [JsonPropertyName("pck_name")]
        public string? PckName { get; set; }

        public string? Author { get; set; }

        public string? Description { get; set; }

        public string? Version { get; set; }

        [JsonIgnore]
        public bool IsEnabled { get; set; } = true;

        [JsonIgnore]
        public string? DirectoryPath { get; set; }
    }
}

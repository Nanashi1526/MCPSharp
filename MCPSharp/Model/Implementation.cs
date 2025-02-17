using System.Text.Json.Serialization;

namespace MCPSharp.Model
{
    public class Implementation
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "MCPSharp Server";

        [JsonPropertyName("version")]
        public string Version { get; set; } = "0.0.1";
    }
}

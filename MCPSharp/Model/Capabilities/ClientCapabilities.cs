using System.Text.Json.Serialization;

namespace MCPSharp.Model.Capabilities
{
    public class ClientCapabilities
    {
        [JsonPropertyName("roots")]
        public RootsCapabilities Roots { get; set; }

        [JsonPropertyName("sampling")]
        public object Sampling { get; set; }
        [JsonPropertyName("tools")]
        public bool Tools { get; set; }

        [JsonPropertyName("resources")]
        public bool Resources { get; set; }

        [JsonPropertyName("prompts")]
        public bool Prompts { get; set; }
    }
}

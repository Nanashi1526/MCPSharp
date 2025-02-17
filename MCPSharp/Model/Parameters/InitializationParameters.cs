using MCPSharp.Model.Capabilities;
using System.Text.Json.Serialization;

namespace MCPSharp.Model.Parameters
{
    internal class InitializationParameters
    {
        [JsonPropertyName("version")]
        public required string Version { get; set; }

        [JsonPropertyName("protocolVersion")]
        public required string ProtocolVersion { get; set; }

        [JsonPropertyName("capabilities")]
        public required ClientCapabilities Capabilities { get; set; }

        [JsonPropertyName("clientInfo")]
        public required Implementation ClientInfo { get; set; }
        
    }
}

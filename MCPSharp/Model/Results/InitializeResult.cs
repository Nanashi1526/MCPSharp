using MCPSharp.Model.Capabilities;
using System.Text.Json.Serialization;

namespace MCPSharp.Model.Results
{
    public class InitializeResult
    {
        [JsonPropertyName("protocolVersion")]
        public string ProtocolVersion { get; set; }

        [JsonPropertyName("capabilities")]
        public ServerCapabilities Capabilities { get; set; }

        [JsonPropertyName("serverInfo")]
        public Implementation ServerInfo { get; set; }
    }
}

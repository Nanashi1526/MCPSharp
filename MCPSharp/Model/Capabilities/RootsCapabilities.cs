using System.Text.Json.Serialization;

namespace MCPSharp.Model.Capabilities
{
    public class RootsCapabilities
    {
        [JsonPropertyName("listChanged")]
        public bool ListChanged { get; set; }
    }
}

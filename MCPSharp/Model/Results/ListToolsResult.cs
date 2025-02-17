using System.Text.Json.Serialization;

namespace MCPSharp.Model.Results
{
    public class ListToolsResult
    {
        [JsonPropertyName("tools")]
        public List<Tool> Tools { get; set; }

        [JsonPropertyName("_meta")]
        public Dictionary<string, object> Meta { get; set; }
    }
}

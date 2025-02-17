using MCPSharp.Model.Schemas;
using System.Text.Json.Serialization;

namespace MCPSharp.Model
{

    public class Tool
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("inputSchema")]
        public InputSchema InputSchema { get; set; }
    }

}

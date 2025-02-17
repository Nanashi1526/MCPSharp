using System.Text.Json.Serialization;

namespace MCPSharp.Model.Schemas
{
    public class InputSchema
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "object";

        [JsonPropertyName("properties")]
        public Dictionary<string, ParameterSchema> Properties { get; set; }

        [JsonIgnore]
        public List<string> Required { get; set; }
    }
}

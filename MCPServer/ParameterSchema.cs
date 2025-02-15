using System.Text.Json.Serialization;

namespace ModelContextProtocol
{
    public class ParameterSchema
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("required")]
        public bool Required { get; set; }
    }
}

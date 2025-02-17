using System.Text.Json.Serialization;

namespace MCPSharp
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class McpParameterAttribute : Attribute
    {
        [JsonPropertyName("required")]
        public bool Required { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        public McpParameterAttribute(bool required = false, string? description = null)
        {
            Required = required;
            Description = description;
        }
    }
}

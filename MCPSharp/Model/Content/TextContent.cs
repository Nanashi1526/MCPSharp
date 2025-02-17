using System.Text.Json.Serialization;

namespace MCPSharp.Model.Content
{
    public class TextContent
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; } = "text";
    }
}

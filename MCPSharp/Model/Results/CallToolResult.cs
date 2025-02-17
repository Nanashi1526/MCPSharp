using MCPSharp.Model.Content;
using System.Text.Json.Serialization;

namespace MCPSharp.Model.Results
{
    public class CallToolResult
    {
        [JsonPropertyName("isError")]
        public bool IsError { get; set; }

        [JsonPropertyName("content")]
        public TextContent[] Content { get; set; }
    }
}

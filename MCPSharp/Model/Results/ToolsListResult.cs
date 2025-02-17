using System.Text.Json.Serialization;

namespace MCPSharp.Model.Results
{
    public class ToolsListResult
    {
        [JsonPropertyName("tools")]
        public List<Tool> Tools { get; set; }
    }
}

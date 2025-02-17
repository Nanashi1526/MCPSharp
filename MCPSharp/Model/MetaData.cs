using System.Text.Json.Serialization;

namespace MCPSharp.Model
{
    public class MetaData
    {
        [JsonPropertyName("progressToken")]
        public int ProgressToken { get; set; }
    }
}

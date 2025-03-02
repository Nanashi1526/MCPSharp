using System.Text.Json.Serialization;

namespace MCPSharp.Model.Parameters
{
    /// <summary>
    /// Represents the parameters for a tool call.
    /// </summary>
    public class ToolCallParameters
    {
        /// <summary>
        /// The name of the tool being called
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// the arguments supplied to the tool
        /// </summary>
        public Dictionary<string, object> Arguments { get; set; } = [];

        /// <summary>
        /// metadata
        /// </summary>
        [JsonPropertyName("_meta")]
        public MetaData Meta { get; set; } = new();
    }
}

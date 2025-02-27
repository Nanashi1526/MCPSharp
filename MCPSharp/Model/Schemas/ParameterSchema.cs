using Newtonsoft.Json.Schema;
using System.Text.Json.Serialization;

namespace MCPSharp.Model.Schemas
{
    /// <summary>
    /// Represents the schema for a parameter.
    /// </summary>
    public class ParameterSchema
    {
        /// <summary>
        /// Gets or sets the type of the parameter.
        /// </summary>
        [JsonPropertyName("type")]
        public required string Type { get; set; }

        /// <summary>
        /// Gets or sets the description of the parameter.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        /// <summary>
        /// Gets or sets a value indicating whether the parameter is required.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        public ParameterSchema Contents { get; set; } = null;

        [JsonIgnore]
        public bool Required { get; set; } = false;
    }
}

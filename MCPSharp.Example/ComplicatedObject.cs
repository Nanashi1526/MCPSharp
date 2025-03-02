using System.Text.Json.Serialization;

namespace MCPSharp.Example
{
    /// <summary>
    /// A complicated object
    /// </summary>
    public class ComplicatedObject()
    {
        /// <summary>The name of the object</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        /// <summary>The age of the object</summary>
        [JsonPropertyName("age")]
        public int Age { get; set; } = 0;

        /// <summary>The hobbies of the object</summary>
        [JsonPropertyName("hobbies")]
        public string[] Hobbies { get; set; } = [];
    }
}
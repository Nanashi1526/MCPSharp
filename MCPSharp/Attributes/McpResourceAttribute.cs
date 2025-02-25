namespace MCPSharp
{
    [AttributeUsage(AttributeTargets.All)]
    public class McpResourceAttribute(string name = null, string uri=null, string mimeType = null, string description = null) : Attribute
    {
        public string Name { get; set; } = name;
        public string Description { get; set; } = description;
        public string Uri { get; set; } = uri;
        public string MimeType { get; set; } = mimeType;
    }
}

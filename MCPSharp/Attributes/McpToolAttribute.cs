namespace MCPSharp
{
    [AttributeUsage(AttributeTargets.Class)]
    public class McpToolAttribute : Attribute
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        public McpToolAttribute(string? name = null, string? description = null)
        {
            Name = name;
            Description = description;
        }
    }
}

namespace MCPSharp
{
    [AttributeUsage(AttributeTargets.Method)]
    public class McpFunctionAttribute : Attribute
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        public McpFunctionAttribute(string? name = null, string? description = null)
        {
            Name = name;
            Description = description;
        }
    }
}

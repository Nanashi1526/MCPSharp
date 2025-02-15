namespace ModelContextProtocol
{
    [AttributeUsage(AttributeTargets.Method)]
    public class McpFunctionAttribute : Attribute
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        public McpFunctionAttribute()
        {
            // Name and Description will be set in RegisterTool
        }
    }
}
 
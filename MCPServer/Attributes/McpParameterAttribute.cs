namespace ModelContextProtocol
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class McpParameterAttribute : Attribute
    {
        public string? Description { get; set; }
        public bool Required { get; set; }

        public McpParameterAttribute(bool required = true)
        {
            Required = required;
        }
    }
}

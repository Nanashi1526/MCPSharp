namespace MCPSharp.ExternalExample
{

    [McpTool("external_tools", "for testing accessing tool classes loaded from a library")]
    public class ExternalTool
    {

        [McpFunction("dll-tool", "attempts to use a tool that is loaded from an external assembly dll. should return 'success'")]
        public static async Task<string> UseAsync() 
        {
            return await Task.Run(()=>"success");
        }

    }
}

using ModelContextProtocol;
using System.Text.Json.Serialization;

var server = new MCPServer();
server.RegisterTool<MCPDev>();
await server.Start();

///<summary>testing interface for custom .net mcp server</summary>
[McpTool][method: JsonConstructor]
public class MCPDev()
{
    ///<summary>just returns a message for testing.</summary>
    [McpFunction] 
    public static string Hello()=> "hello, claude.";

    ///<summary>returns ths input string back</summary>
    ///<param name="input">the string to echo</param>
    [McpFunction] 
    public static string Echo([McpParameter(true)] string input) => input;
}
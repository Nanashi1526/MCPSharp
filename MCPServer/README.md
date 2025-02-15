# MCP Server Library

This library provides a framework for creating and running a custom .NET MCP (Model Context Protocol) server. It allows you to define tools and functions that can be invoked via the MCP protocol.

## Requirements

- .NET 9
- C# 13.0

## Getting Started

### Installation

1. Clone the repository:
    
``` 
    git clone https://github.com/your-repo/mcp-server-library.git
``` 

2. Navigate to the project directory:
    
``` 
    cd mcp-server-library
``` 

3. Build the project:
    
``` 
    dotnet build
``` 

### Usage

1. Create a new class and mark it with the `[McpTool]` attribute to define a tool. Use the `[McpFunction]` attribute to define functions within the tool. Use the `[McpParameter]` attribute to define parameters for the functions.

``` 
    using ModelContextProtocol;
    using System.Text.Json.Serialization;

    [McpTool("example_tool", "An example tool for the MCP server")]
    [JsonSerializable(typeof(ExampleTool))]
    public class ExampleTool
    {
        [McpFunction("greet", "Returns a greeting message.")]
        public static string Greet([McpParameter("name", true)] string name)
        {
            return $"Hello, {name}!";
        }
    }
``` 

2. Register the tool with the server and start the server:

``` 
    using ModelContextProtocol;
    using System.Text.Json.Serialization;

    var server = new MyMcpServer();
    server.RegisterTool<ExampleTool>();
    await server.Start();
``` 

3. Run the application:

``` 
    dotnet run
``` 

### Example

Here is a complete example:

``` 
using ModelContextProtocol;
using System.Text.Json.Serialization;

var server = new MyMcpServer();
server.RegisterTool<ExampleTool>();
await server.Start();

[McpTool("example_tool", "An example tool for the MCP server")]
[JsonSerializable(typeof(ExampleTool))]
public class ExampleTool
{
    [McpFunction("greet", "Returns a greeting message.")]
    public static string Greet([McpParameter("name", true)] string name)
    {
        return $"Hello, {name}!";
    }
}
``` 

### Attributes

- `[McpTool]`: Marks a class as an MCP tool provider.
- `[McpFunction]`: Marks a method as an MCP function.
- `[McpParameter]`: Marks a parameter with additional MCP metadata.

### Contributing

Contributions are welcome! Please open an issue or submit a pull request.

### License

This project is licensed under the MIT License.

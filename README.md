# MCPSharp

MCPSharp is a .NET library that helps you build Model Context Protocol (MCP) servers - the standardized API protocol used by AI assistants and models. With MCPSharp, you can:

- Create MCP-compliant tools and functions that AI models can discover and use
- Expose your .NET methods as MCP endpoints with simple attributes
- Generate accurate MCP schema documentation automatically
- Handle MCP protocol details and JSON-RPC communication seamlessly

## When to Use MCPSharp

Use MCPSharp when you want to:
- Create tools that AI assistants like Anthropic's Claude Desktop can use
- Build MCP-compliant APIs without dealing with the protocol details
- Expose existing .NET code as MCP endpoints
- Add AI capabilities to your applications through standardized interfaces

## Features

- Easy-to-use attribute-based API (`[McpTool]`, `[McpFunction]`)
- Built-in JSON-RPC support with automatic request/response handling
- Automatic parameter validation and type conversion
- Rich documentation support through XML comments
- Zero configuration required for basic usage

## Prerequisites

- any version of .NET that supports [standard 2.0](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0#tabpanel_1_net-standard-2-0)
  

## Installation

```bash
dotnet add package MCPSharp
```

## Quick Start

### 1. Define a Tool

Create a class and mark it with the `[McpTool]` attribute:

```csharp
using MCPSharp;

[McpTool]
public class Calculator
{
    [McpFunction]
    public static int Add([McpParameter(true)] int a, [McpParameter(true)] int b)
    {
        return a + b;
    }
}
```

Then just start the server:
```csharp
await MCPServer.StartAsync("CalculatorServer", "1.0.0");
```

## API Reference

### Attributes

- `[McpTool]` - Marks a class as an MCP tool
    -  Optional parameters:
        - `Name` - The tool name (default: class name)
        - `Description` - A description of the tool. This should be used to provide additional 
          context to the tool. Will use the XML summary (see relevant section) if not provided.)
        
- `[McpFunction]` - Marks a method as an MCP function
    - Optional parameters:
        - `Name` - The function name (default: method name)
        - `Description` - A description of the function. This should be used to provide additional 
           context to the function. Will use the XML summary (see relevant section) if not provided.)
- `[McpParameter]` - Provides metadata for function parameters
    - Optional parameters:
        - `Description` - A description of the parameter. This should be used to provide additional 
            context to the parameter.
        - `Required` - Whether the parameter is required (default: false)

### Additional Methods

- `MCPServer.StartAsync(string toolName, string toolVersion)` - Starts the MCP server with the specified 
              tool name and version. This method should be called to start the server.

  - `MCPServer.RegisterTool<T>()` - Registers a tool with the server. This method should be called before 
        starting the server, IF you wish to register a tool that is loaded from an external class library, 
        or a semantic kernel function. 
     Any `[McpTool]` that are part of the main assembly will be automatically registered.
    - `T` can be either a class that has the `[McpTool]` attrubutes, OR it can be a class defined with 
[Semantic Kernel's KernelFunction Attributes](https://learn.microsoft.com/en-us/dotnet/api/microsoft.semantickernel.kernelfunctionattribute?view=semantic-kernel-dotnet)
      - `MCPServer.RegisterTool<MyMcpToolClass>();`

### Client
There is a client class included as well, designed to help with testing. It is provided as is, and is 
not extremely flexible. I would suggest anyone wanting a client to check out 
[mcpdotnet](https://github.com/PederHP/mcpdotnet), as it seems to be much more feature complete. If you 
would like to use the MCPSharp implementation anyway, check out the MCPSharp.Test project for how it works. 


## Example

Here's a complete example:

```csharp
using MCPSharp;

MCPServer.StartAsync("StringTools", "1.0.0");

[McpTool("string_tools", "String manipulation utilities")]
public class StringTools
{
    [McpFunction("concat", "Concatenates two strings with separator")]
    public static string Concat(
        [McpParameter(true)] string first,
        [McpParameter(true)] string second,
        [McpParameter(Description = "Separator between strings")] string separator = " "
    )
    {
        return $"{first}{separator}{second}";
    }
}
```

## XML Documentation Support

MCPSharp can automatically extract documentation from XML comments to provide rich descriptions for your tools and functions in the MCP schema.

### Enable XML Documentation

1. In your project file (`.csproj`), add or modify the following settings:

```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

### Adding Documentation

Use standard XML documentation comments on your tools and functions:

```csharp
/// <summary>
/// Provides mathematical operations
/// </summary>
[McpTool("calculator")]
public class Calculator
{
    /// <summary>
    /// Adds two numbers together
    /// </summary>
    /// <param name="a">The first number to add</param>
    /// <param name="b">The second number to add</param>
    /// <returns>The sum of the two numbers</returns>
    [McpFunction]
    public static int Add(
        [McpParameter(true)] int a,
        [McpParameter(true)] int b)
    {
        return a + b;
    }
}
```

### Documentation Priority

MCPSharp uses the following priority order when determining descriptions:

1. Explicit descriptions in attributes (`[McpTool(Description = "...")]`)
2. XML documentation comments (if the XML documentation file is present)

## Roadmap

We have several features planned for future releases of MCPSharp:

- **Complex Object Parameter Parsing**: Properly parse input/return parameters that are complicated objects. Currently they can be used, but the library will not expose the shape of the object to the client, so it must be mentioned in the description somewhere.
- **Tool Change Notifications**: Implement notifications for tool changes (updates client when tools are added/updated/deleted)
- **Standardized Error Handling**: Standardize error handling across the library.
- **Additional Endpoints**: Implement the rest of the endpoints like resources, etc.
- **SSE Transport**: enable the use of Server-Sent Events as a transport mechanism.
- 


### IntelliSense Support

The XML documentation also provides IntelliSense support in IDEs when consuming your MCP tools as a library.

## Contributing

We welcome contributions! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License.
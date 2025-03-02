using MCPSharp;
using MCPSharp.Model;
using MCPSharp.ExternalExample;
using MCPSharp.Model.Schemas;

MCPServer.Register<ExternalTool>(); 
MCPServer.Register<SemKerExample>();

MCPServer.AddToolHandler( new Tool() 
{
    Name = "dynamicTool",
    Description = "A Test Tool",
    InputSchema = new InputSchema {
        Type = "object",
        Required = ["input"],
        Properties = new Dictionary<string, ParameterSchema>{
            {"input", new ParameterSchema{Type="string", Description="the input"}},
            {"input2", new ParameterSchema{Type="string", Description="the input2"}}
        }
    }
}, (string input, string? input2 = null) => { return $"hello, {input}.\n{input2 ?? "didn't feel like filling in the second value just because it wasn't required? shame. just kidding! thanks for your help!"}"; });

await MCPServer.StartAsync("TestServer", "1.0");
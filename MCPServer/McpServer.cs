using ModelContextProtocol.NET.Core.Models.Protocol.Client.Responses;
using ModelContextProtocol.NET.Core.Models.Protocol.Common;
using ModelContextProtocol.NET.Core.Models.Protocol.Shared.Content;
using ModelContextProtocol.NET.Server;
using ModelContextProtocol.NET.Server.Builder;
using ModelContextProtocol.NET.Server.Features.Tools;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace ModelContextProtocol
{

    public class MCPServer
    {
        private readonly McpServerBuilder builder;
        private IMcpServer? server;

        public MCPServer(string ServerName = "TEST", string Version = "0.0.0")
        {
           Implementation serverInfo; 
            serverInfo = new() { Name = ServerName, Version = Version };
            builder = new McpServerBuilder(serverInfo).AddStdioTransport();
        }

        public void RegisterTool<T>() where T : class, new()
        {
            var type = typeof(T);
            var toolAttr = type.GetCustomAttribute<McpToolAttribute>();
            if (toolAttr == null)
            {
                toolAttr = new McpToolAttribute
                {
                    Name = type.Name,
                    Description = type.GetXmlDocumentation()
                };
            }

            foreach (var method in type.GetMethods())
            {
                var methodAttr = method.GetCustomAttribute<McpFunctionAttribute>();
                if (methodAttr == null) continue;

                // Set name and description from method info
                methodAttr.Name ??= method.Name;
                methodAttr.Description ??= method.GetXmlDocumentation();

                var parameterSchemas = method.GetParameters().ToDictionary(
                    p => p.Name!,
                    p => new ParameterSchema
                    {
                        Type = GetJsonSchemaType(p.ParameterType),
                        Description = p.GetCustomAttribute<McpParameterAttribute>()?.Description ?? p.GetXmlDocumentation() ?? "",
                        Required = p.GetCustomAttribute<McpParameterAttribute>()?.Required ?? true,
                    }
                );

                var tool = new Tool
                {
                    Name = methodAttr.Name,
                    Description = methodAttr.Description,
                    InputSchema = new ToolInputSchema
                    {
                        Properties = parameterSchemas.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value)
                    }
                };

                var handler = new ToolHandler
                {
                    Tool = tool,
                    JsonTypeInfo = JsonTypeInfo.CreateJsonTypeInfo<T>(new JsonSerializerOptions
                    {
                        TypeInfoResolver = McpSerializerContext.Default,
                        UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode,
                        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
                    }),
                    HandlerFunction = async (args, ct) =>
                    {
                        try
                        {
                            var paramValues = method.GetParameters().Select(p =>
                            {
                                if (!args.TryGetProperty(p.Name!, out var value))
                                {
                                    if (p.GetCustomAttribute<McpParameterAttribute>()?.Required ?? true)
                                    {
                                        throw new ArgumentException($"Required parameter '{p.Name}' not provided");
                                    }
                                    return p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null;
                                }
                                return JsonSerializer.Deserialize(value, p.ParameterType,
                                    new JsonSerializerOptions { TypeInfoResolver = McpSerializerContext.Default });
                            }).ToArray();

                            var instance = new T();
                            var result = method.Invoke(instance, paramValues);

                            if (result is Task task)
                            {
                                await task.ConfigureAwait(false);
                                var resultProperty = task.GetType().GetProperty("Result");
                                result = resultProperty?.GetValue(task);
                            }

                            return new CallToolResult
                            {
                                Content = new[] { new TextContent { Text = result?.ToString() ?? string.Empty } }
                            };
                        }
                        catch (Exception ex)
                        {
                            return new CallToolResult
                            {
                                IsError = true,
                                Content = new[] { new TextContent { Text = $"Exception in HandlerFunction: tool: {tool}\n{ex.Message}\n\nSTACK\n{ex.StackTrace}" } }
                            };
                        }
                    }
                };

                builder.Tools.AddHandler(handler);
                builder.Tools.AddFunction(
                    tool.Name,
                    tool.Description,
                    JsonTypeInfo.CreateJsonTypeInfo<Dictionary<string, object>>(new JsonSerializerOptions
                    {
                        TypeInfoResolver = McpSerializerContext.Default
                    }),
                    handler.HandleAsync
                );
            }
        }


        private static string GetJsonSchemaType(Type type)
        {
            return type switch
            {
                Type t when t == typeof(string) => "string",
                Type t when t == typeof(int) || t == typeof(double) || t == typeof(float) => "number",
                Type t when t == typeof(bool) => "boolean",
                Type t when t.IsArray => "array",
                Type t when t == typeof(DateTime) => "string",
                _ => "object"
            };
        }

        private class ToolHandler : IToolHandler
        {
            public required Tool Tool { get; set; }
            public required JsonTypeInfo JsonTypeInfo { get; set; }
            public required Func<JsonElement, CancellationToken, Task<CallToolResult>> HandlerFunction { get; set; }

            public Task<CallToolResult> HandleAsync(object parameters, CancellationToken cancellationToken = default)
            {
                if (parameters is JsonElement jsonElement)
                {
                    return HandlerFunction(jsonElement, cancellationToken);
                }
                
                if (parameters is Dictionary<string, object> dictionary)
                {
                    if (dictionary.Count == 0) 
                        return HandlerFunction(JsonDocument.Parse("{}").RootElement, cancellationToken);//idk


                    StringBuilder sb = new();
                    foreach (var (key, value) in dictionary)
                    {
                        sb.Append($"{key}: {value}\n");
                    }

                    throw new Exception($"Opps we got a dictionary!: {sb}");
                    var s = dictionary.ToArray();

                    //return HandlerFunction(s, cancellationToken);
                }
                throw new ArgumentException("Invalid parameters type");
            }
        }

        public async Task Start()
        {
            server = builder.Build();
            server.Start();
        
            await Task.Delay(-1);
        }
    }
}

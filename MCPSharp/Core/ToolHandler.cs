using MCPSharp.Model;
using MCPSharp.Model.Content;
using MCPSharp.Model.Results;
using System.Reflection;
using System.Text.Json;

namespace MCPSharp.Core
{
    internal class ToolHandler<T>(Tool tool, MethodInfo method) where T : class, new()
    {
        private readonly Tool _tool = tool;
        private readonly MethodInfo _method = method;

        public Tool GetToolDefinition()
        {
            return new Tool
            {
                Name = _tool.Name,
                Description = _tool.Description,
                InputSchema = _tool.InputSchema
            };
        }

        public async Task<CallToolResult> HandleAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
        {
            try
            {
                var instance = new T();
                var inputValues = new Dictionary<string, object>();

                foreach (var item in parameters)
                {
                    if (item.Value is JsonElement jsonElement)
                    {
                        inputValues.Add(item.Key, JsonSerializer.Deserialize(jsonElement.GetRawText()!, _method.GetParameters().FirstOrDefault(p => p.Name == item.Key)!.ParameterType!)!);
                    }
                }

                var result = _method.Invoke(instance, [.. inputValues.Values]);

                if (result is Task task)
                {
                    await task.ConfigureAwait(false);
                    var resultProperty = task.GetType().GetProperty("Result");
                    result = resultProperty?.GetValue(task);
                }

                return new CallToolResult
                {
                    Content = [new TextContent { Type = "text", Text = result?.ToString() ?? "no response" }]
                };
            }
            catch (Exception ex)
            {
                return new CallToolResult
                {
                    IsError = true,
                    Content = [new TextContent { Text = $"Exception in HandlerFunction: tool: {_tool.Name}\n{ex.Message}\n\nSTACK\n{ex.StackTrace}" }]
                };
            }
        }
    }
}

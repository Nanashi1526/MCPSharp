using MCPSharp.Model;
using MCPSharp.Model.Content;
using MCPSharp.Model.Results;
using System.Reflection;
using System.Text.Json;

namespace MCPSharp.Core.Tools
{
    public class ToolHandler(Tool tool, MethodInfo method)
    {
        public Tool Tool = tool;
        private readonly MethodInfo _method = method;
       
        public async Task<CallToolResult> HandleAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
        {
            try
            {
                var inputValues = new Dictionary<string, object>();

                foreach (var item in parameters)
                {
                    if (item.Value is JsonElement jsonElement)
                    {
                        var val = JsonSerializer.Deserialize(jsonElement.GetRawText()!,
                            _method.GetParameters().FirstOrDefault(p => p.Name == item.Key)!.ParameterType!)!;

                        inputValues.Add(item.Key, val);
                    }
                }

                var result = _method.Invoke(Activator.CreateInstance(_method.DeclaringType), [.. inputValues.Values]);

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
                var e = ex is TargetInvocationException tie ? tie.InnerException ?? tie : ex;
                var stackTrace = e.StackTrace?.Split([Environment.NewLine], StringSplitOptions.None)
                                              .Where(line => !line.Contains("System.RuntimeMethodHandle.InvokeMethod") 
                                                          && !line.Contains("System.Reflection.MethodBaseInvoker.InvokeWithNoArgs")).ToArray();

                return new CallToolResult
                {
                    IsError = true,
                    Content =
                    [
                        new TextContent { Text = $"{e.Message}" },
                        new TextContent { Text = $"StackTrace:\n{string.Join("\n", stackTrace)}" }
                    ]
                };
            }
        }
    }
}

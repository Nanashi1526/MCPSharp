using MCPSharp.Model.Schemas;
using MCPSharp.Model;
using Microsoft.SemanticKernel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Reflection;

namespace MCPSharp.Core.Tools
{
    class ToolManager
    {
        public readonly Dictionary<string, ToolHandler<object>> Tools = new();
        public readonly List<Resource> Resources = new();


        /// <summary>
        /// Registers a tool with the server.
        /// </summary>
        public void RegisterTool<T>() where T : class, new()
        {
            var type = typeof(T);
            var toolAttr = type.GetCustomAttribute<McpToolAttribute>() ?? new McpToolAttribute { Name = type.Name, Description = type.GetXmlDocumentation() };

            foreach (var method in type.GetMethods())
            {
                var methodAttr = method.GetCustomAttribute<McpFunctionAttribute>();
                if (methodAttr != null)
                {


                    methodAttr.Name ??= method.Name;
                    methodAttr.Description ??= method.GetXmlDocumentation();

                    var parameterSchemas = method.GetParameters().ToDictionary(
                        p => p.Name!,
                        p => new ParameterSchema
                        {
                            Type = p.ParameterType switch
                            {
                                Type t when t == typeof(string) => "string",
                                Type t when t == typeof(int) || t == typeof(double) || t == typeof(float) => "number",
                                Type t when t == typeof(bool) => "boolean",
                                Type t when t.IsArray => "array",
                                Type t when t == typeof(DateTime) => "string",
                                _ => "object"
                            },
                            Description = p.GetXmlDocumentation() ?? p.GetCustomAttribute<McpParameterAttribute>()?.Description ?? "",
                            Required = p.GetCustomAttribute<McpParameterAttribute>()?.Required ?? false,
                        }
                    );

                    Tools[methodAttr.Name] = new ToolHandler<object>(new Tool
                    {
                        Name = methodAttr.Name,
                        Description = methodAttr.Description ?? "",
                        InputSchema = new InputSchema
                        {
                            Properties = parameterSchemas,
                            Required = parameterSchemas.Where(kvp => kvp.Value.Required).Select(kvp => kvp.Key).ToList(),
                        }
                    }, method!);


                }
                else
                {
                    var resAttr = method.GetCustomAttribute<McpResourceAttribute>();
                    if (resAttr != null)
                    {
                        // Resource registrationre
                        Resources.Add(new Resource() { Name = resAttr.Name, Description = resAttr.Description, Uri = resAttr.Uri, MimeType = resAttr.MimeType });
                        //[method.Name] = method.GetCustomAttribute<McpResourceAttribute>().Name ?? method.Name;

                    }
                    else
                    {

                        //check for semker
                        var kernelFunctionAttribute = method.GetCustomAttribute<KernelFunctionAttribute>();

                        if (kernelFunctionAttribute == null)
                            continue;

                        var parameterSchemas = method.GetParameters().ToDictionary(
                            p => p.Name!,
                            p => new ParameterSchema
                            {
                                Type = p.ParameterType switch
                                {
                                    Type t when t == typeof(string) => "string",
                                    Type t when t == typeof(int) || t == typeof(double) || t == typeof(float) => "number",
                                    Type t when t == typeof(bool) => "boolean",
                                    Type t when t.IsArray => "array",
                                    Type t when t == typeof(DateTime) => "string",
                                    _ => "object"
                                },
                                Description = p.GetXmlDocumentation() ?? p.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "",
                                Required = p.GetCustomAttribute<RequiredAttribute>() != null,
                            }
                        );

                        Tools[kernelFunctionAttribute.Name] = new ToolHandler<object>(new Tool
                        {
                            Name = kernelFunctionAttribute.Name,
                            Description = method.GetCustomAttribute<DescriptionAttribute>().Description ?? "",
                            InputSchema = new InputSchema
                            {
                                Properties = parameterSchemas,
                                Required = parameterSchemas.Where(kvp => kvp.Value.Required).Select(kvp => kvp.Key).ToList(),
                            }
                        }, method!);

                    }
                }
            }

            foreach (var property in type.GetProperties())
            {
                var resAttr = property.GetCustomAttribute<McpResourceAttribute>();
                if (resAttr != null)
                {
                    // Resource registrationre
                    Resources.Add(new Resource() { Name = resAttr.Name, Description = resAttr.Description, Uri = resAttr.Uri, MimeType = resAttr.MimeType });
                    //[method.Name] = method.GetCustomAttribute<McpResourceAttribute>().Name ?? method.Name;

                }
            }
        }

    }
}

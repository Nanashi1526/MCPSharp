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
        public readonly Dictionary<string, ToolHandler> Tools = [];
        public readonly List<Resource> Resources = [];


        /// <summary>
        /// Registers a tool with the server.
        /// </summary>
        public void RegisterTool<T>() where T : class, new()
        {
            var type = typeof(T);
            
            foreach (var method in type.GetMethods())
            {
                //these both will exit early if they don't find the right attribute
                RegisterMcpFunction(method);
                RegisterSemanticKernelFunction(method);
            }

            foreach (var property in type.GetProperties())
            {
                var resAttr = property.GetCustomAttribute<McpResourceAttribute>();
                if (resAttr != null)
                {
                    Resources.Add(new Resource() { 
                        Name = resAttr.Name, 
                        Description = resAttr.Description, 
                        Uri = resAttr.Uri, 
                        MimeType = resAttr.MimeType 
                    });   
                }
            }
        }

        public void AddToolHandler(ToolHandler tool) 
        {
            Tools[tool.Tool.Name] = tool;
        }

        private void RegisterSemanticKernelFunction(MethodInfo method)
        {
            var kernelFunctionAttribute = method.GetCustomAttribute<KernelFunctionAttribute>();
            if (kernelFunctionAttribute == null) return;

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

            Tools[kernelFunctionAttribute.Name] = new ToolHandler(new Tool
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

        private void RegisterMcpFunction(MethodInfo method)
        {
            string name = "";
            string description = "";

            var mcpFuncAttr = method.GetCustomAttribute<McpFunctionAttribute>();
            if (mcpFuncAttr != null)
            {
                name = mcpFuncAttr.Name ?? method.Name;
                description = mcpFuncAttr.Description ?? method.GetXmlDocumentation(); 
            }
            else
            {
                var methodAttr = method.GetCustomAttribute<McpToolAttribute>();
                if (methodAttr != null)
                {
                    name = methodAttr.Name ?? method.Name;
                    description = methodAttr.Description ?? method.GetXmlDocumentation();
                }
                else { return; }
            }
           

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
                    Contents = p.ParameterType.IsArray ? new ParameterSchema
                    {
                        Type = p.ParameterType.GetElementType()!.Name,
                        Description = p.GetXmlDocumentation() ?? p.GetCustomAttribute<McpParameterAttribute>()?.Description ?? "",
                        Required = p.GetCustomAttribute<McpParameterAttribute>()?.Required ?? false,
                    } : null
                }
            );

            Tools[name] = new ToolHandler(new Tool
            {
                Name = name,
                Description = description ?? "",
                InputSchema = new InputSchema
                {
                    Properties = parameterSchemas,
                    Required = parameterSchemas.Where(kvp => kvp.Value.Required).Select(kvp => kvp.Key).ToList(),
                }
            }, method!);
        }
    }
}

using ModelContextProtocol.NET.Core.Models.Protocol.Client.Responses;
using ModelContextProtocol.NET.Core.Models.Protocol.Common;
using ModelContextProtocol.NET.Core.Models.Protocol.Shared.Content;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ModelContextProtocol
{
    [JsonSerializable(typeof(ParameterSchema))]
    [JsonSerializable(typeof(object))]
    [JsonSerializable(typeof(Dictionary<string, ParameterSchema>))]
    [JsonSerializable(typeof(Tool))]
    [JsonSerializable(typeof(ToolInputSchema))]
    [JsonSerializable(typeof(CallToolResult))]
    [JsonSerializable(typeof(Annotated[]))]
    [JsonSerializable(typeof(TextContent))]
    [JsonSerializable(typeof(Dictionary<string, object>))]
    [JsonSerializable(typeof(JsonObject))] 
    internal partial class McpSerializerContext : JsonSerializerContext 
    { 

    }
}

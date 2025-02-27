using MCPSharp.Core.Tools;
using MCPSharp.Core.Transport;
using MCPSharp.Core.Transport.SSE;
using MCPSharp.Model;
using MCPSharp.Model.Capabilities;
using MCPSharp.Model.Parameters;
using MCPSharp.Model.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using StreamJsonRpc;
using System.Reflection;
using System.Text;

namespace MCPSharp
{
    /// <summary>
    /// Main class for the MCP server.
    /// </summary>
    public class MCPServer
    {
        private static readonly MCPServer _instance = new();
        private readonly JsonRpc _rpc;
        private readonly Stream StandardOutput;
        private readonly ServerSentEventsService _sseService;
        private readonly ToolManager _toolManager = new();

        /// <summary>
        /// The implementation details of the server.
        /// </summary>
        public Implementation Implementation;

        /// <summary>
        /// The output of Console.WriteLine()
        /// </summary>
        public readonly TextWriter RedirectedOutput = TextWriter.Null;

        /// <summary>
        /// Constructor for the MCP server.
        /// </summary>
        private MCPServer()
        {
            StandardOutput = Console.OpenStandardOutput();
            Console.SetOut(RedirectedOutput);
            var pipe = new DuplexPipe(Console.OpenStandardInput(), StandardOutput);
            _rpc = new JsonRpc(new NewLineDelimitedMessageHandler(pipe, new SystemTextJsonFormatter()), this);
            _sseService = new ServerSentEventsService(CreateSseInstance);
        }

        /// <summary>
        /// Constructor for SSE instances
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        private MCPServer(Stream input, Stream output)
        { 
            var pipe = new DuplexPipe(input, output);
            _rpc = new JsonRpc(new NewLineDelimitedMessageHandler(pipe, new SystemTextJsonFormatter()), this);
            _rpc.StartListening();
        }

        /// <summary>
        /// Constructor for the MCP server with implementation details.
        /// </summary>
        /// <param name="implementation">The implementation details of the server.</param>
        public MCPServer(Implementation implementation) : this() => Implementation = implementation;

        /// <summary>
        /// Constructor for the MCP server with output redirection.
        /// </summary>
        /// <param name="outputWriter">A TextWriter object where any Console.Write() calls will go.</param>
        public MCPServer(TextWriter outputWriter) : this() => RedirectedOutput = outputWriter;

        /// <summary>
        /// Registers a tool with the server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void RegisterTool<T>() where T : class, new() => _instance._toolManager.RegisterTool<T>();


        public static void AddToolHandler(Tool tool, Delegate func)
        {
            _instance._toolManager.AddToolHandler(new ToolHandler(tool, func.Method));
        }


        /// <summary>
        /// Factory method to create new instances for SSE clients
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        private static MCPServer CreateSseInstance(Stream input, Stream output)
        {
            var instance = new MCPServer(input, output);
            //instance.tools = tools;
            
            instance.Implementation = _instance.Implementation;
            return instance;
        }

        /// <summary>
        /// forward Console.WriteLine() to a text file
        /// </summary>
        /// <param name="filename"></param>
        public static void SetOutput(string filename) => Console.SetOut(new StreamWriter(File.OpenWrite(filename)) { AutoFlush = true });

        /// <summary>
        /// forward Console.WriteLine() to a TextWriter
        /// </summary>
        /// <param name="output"></param>
        public static void SetOutput(TextWriter output) => Console.SetOut(output);

        /// <summary>
        /// Starts the MCP Server, registers all tools, and starts listening for requests.
        /// </summary>
        /// <param name="serverName">The name of the server.</param>
        /// <param name="version">The version of the server.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task StartAsync(string serverName, string version)
        {
            _instance.Implementation = new(serverName, version);

            
            var allTypes = Assembly.GetEntryAssembly()!.GetTypes()
                .Where(t => {
                    bool classHasToolAttribute = t.GetCustomAttribute<McpToolAttribute>() != null;
                    bool methodHasToolAttribute = t.GetMethods().Any(m => m.GetCustomAttribute<McpFunctionAttribute>() != null);
                    bool methodHasResourceAttribute = t.GetMethods().Any(m => m.GetCustomAttribute<McpResourceAttribute>() != null);

                    return classHasToolAttribute || methodHasToolAttribute || methodHasResourceAttribute;
                    });

            foreach (var toolType in allTypes)
            {
                var registerMethod = typeof(MCPServer).GetMethod(nameof(ToolManager.RegisterTool))?.MakeGenericMethod(toolType);
                registerMethod?.Invoke(_instance, null);
            }

            _instance.Start();

            await Task.Delay(-1);
        }



        /// <summary>
        /// Initializes the server with the specified protocol version, client capabilities, and client information.
        /// </summary>
        /// <param name="protocolVersion">The protocol version.</param>
        /// <param name="capabilities">The client capabilities.</param>
        /// <param name="clientInfo">The client information.</param>
        /// <returns>The result of the initialization process.</returns>
        [JsonRpcMethod("initialize")]
        public InitializeResult Initialize(string protocolVersion, ClientCapabilities capabilities, Implementation clientInfo) => new(protocolVersion, new ServerCapabilities { Tools = new() { { "listChanged", false } } }, Implementation);

        /// <summary>
        /// Handles the "notifications/initialized" JSON-RPC method.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [JsonRpcMethod("notifications/initialized")]
        public static async Task InitializedAsync() => await Task.Run(() => { });

        /// <summary>
        /// Lists the resources available on the server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of resources.</returns>
        [JsonRpcMethod("resources/list")]
        public async Task<ResourcesListResult> ListResourcesAsync() => await Task.Run(() => new ResourcesListResult() { Resources=_toolManager.Resources});

        /// <summary>
        /// Lists the resource templates available on the server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of resource templates.</returns>
        [JsonRpcMethod("resources/templates/list")]
        public async Task<ResourceTemplateListResult> ListResourceTemplatesAsync() => await Task.Run(() => new ResourceTemplateListResult());

        /// <summary>
        /// Calls a tool with the specified parameters.
        /// </summary>
        /// <param name="parameters">The parameters for the tool call.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the result of the tool call.</returns>
        [JsonRpcMethod("tools/call", UseSingleObjectParameterDeserialization = true)]
        public async Task<CallToolResult> CallToolAsync(ToolCallParameters parameters) => 
            !_toolManager.Tools.TryGetValue(parameters.Name, out var toolHandler) ? 
                new CallToolResult { 
                    IsError = true, 
                    Content = new[] { new Model.Content.TextContent { Text = $"Tool {parameters.Name} not found" } } ,

                } 
                : await toolHandler.HandleAsync(parameters.Arguments);

        /// <summary>
        /// Lists the tools available on the server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of tools.</returns>
        [JsonRpcMethod("tools/list")]
        public async Task<ToolsListResult> ListToolsAsync(object parameters = null) => 
            await Task.Run(() => new ToolsListResult(_toolManager.Tools.Values.Select(t => t.Tool).ToList()));

        /// <summary>
        /// Pings the server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the ping response.</returns>
        [JsonRpcMethod("ping")]
        public async Task<object> PingAsync() => await Task.Run(() => new { });

        /// <summary>
        /// Lists the prompts available on the server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of prompts.</returns>
        [JsonRpcMethod("prompts/list")]
        public async Task<PromptListResult> ListPromptsAsync() => await Task.Run(() => new PromptListResult());

        /// <summary>
        /// Starts the JSON-RPC listener.
        /// </summary>
        public void Start()
        {
            _rpc.StartListening();

            // Build HTTP server for SSE endpoint
            var builder = new WebHostBuilder().UseKestrel().UseUrls("http://localhost:8080")
                .Configure(app =>
                {
                    app.Run(async context =>
                    {
                        if (context.Request.Path == "/sse")
                        {
                            var response = context.Response;
                            response.Headers.Add("Content-Type", "text/event-stream");
                            response.Headers.Add("Cache-Control", "no-cache");
                            response.Headers.Add("Connection", "keep-alive");

                            context.Response.StatusCode = 200;

                            var outputStream = context.Response.Body;

                            var inputStream = new HttpPostStream($"http://localhost:8080/messages"); 

                            _sseService.AddClientAsync(context, inputStream);

                           
                            await outputStream.WriteAsync(Encoding.UTF8.GetBytes("data: "), 0, 6);
                            await outputStream.FlushAsync();
                            await Task.Delay(1000);
                            await outputStream.WriteAsync(Encoding.UTF8.GetBytes("data: "), 0, 6);
                            await outputStream.FlushAsync();
                           
                        }
                        else if (context.Request.Path == "/messages" && context.Request.Method == "POST")
                        {
                            using var reader = new StreamReader(context.Request.Body);
                            var message = await reader.ReadToEndAsync();

                            // Parse the message, discover the correct client, and forward the message to the correct stream
                            // var client = _sseService.GetClient(context.Request.Body);
                            // await client.SendAsync(message);

                            context.Response.StatusCode = 200;
                        }
                        else
                        {
                            context.Response.StatusCode = 404;
                        }
                    });
                });

            var host = builder.Build();
            _ = host.RunAsync(); // Run async and discard to avoid blocking
        }

        internal void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
        
}
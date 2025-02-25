using MCPSharp.Core.Transport;
using MCPSharp.Core.Transport.SSE;
using MCPSharp.Model;
using MCPSharp.Model.Parameters;
using MCPSharp.Model.Results;
using StreamJsonRpc;
using System.Diagnostics;

using System.Net.Http.Headers;

namespace MCPSharp
{
    /// <summary>
    /// MCPSharp Model Context Protocol Client.
    /// </summary>
    public class MCPClient : IDisposable
    {
        private readonly string _name;
        private readonly string _version;
        private readonly Process _process;
        private JsonRpc _rpc;
        public bool Initialized { get; private set; } = false;

        private HttpClient _httpClient;
        private Stream _stream;
        /// <summary>
        /// The tools that have been registered with the client.
        /// </summary>
        public List<Tool> Tools { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MCPClient"/> class.
        /// </summary>
        /// <param name="name">The name of the client.</param>
        /// <param name="version">The version of the client.</param>
        /// <param name="server">The path to the executable server.</param>
        /// <param name="args">Additional arguments for the server.</param>
        public MCPClient(string name, string version, string server, params string[] args)
        {
            _name = name;
            _version = version;
            _process = new()
            {
                StartInfo = new()
                {
                    FileName = server,
                    Arguments = string.Join(" ", args),
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            _process.Start();
            var pipe = new DuplexPipe(_process.StandardOutput.BaseStream, _process.StandardInput.BaseStream);
            _rpc = new JsonRpc(new NewLineDelimitedMessageHandler(pipe, new SystemTextJsonFormatter()), this);
            _rpc.StartListening();

            _ = _rpc.InvokeAsync<InitializeResult>(
                    "initialize", 
                    [
                        "2024-11-05", 
                        new { 
                            roots = new { listChanged = false }, 
                            sampling = new { } }, 
                        new { 
                            name = _name, 
                            version = _version }
                    ]);

            _ = _rpc.NotifyAsync("notifications/initialized");
            _ = GetToolsAsync();
        }

        public MCPClient(Uri address, string name, string version)
        {
            _name = name;
            _version = version;
            _httpClient = new HttpClient();
            _rpc = new JsonRpc(new NewLineDelimitedMessageHandler(
                new DuplexPipe(_httpClient.GetStreamAsync(address).Result, 
                new HttpPostStream(address.ToString())), new SystemTextJsonFormatter()));
            _rpc.StartListening();
            var result = _rpc.InvokeAsync<InitializeResult>(
                 "initialize",
                 [
                     "2024-11-05",
                        new {
                            roots = new { listChanged = false },
                            sampling = new { } },
                        new {
                            name = _name,
                            version = _version }
                 ]);
            result.Wait();
            //Console.WriteLine(result.Result.ServerInfo.Name);

            _ = _rpc.NotifyAsync("notifications/initialized");
            Initialized = true;
        }
        /// <summary>
        /// resets the connection to an SSE connection 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public async Task InitializeSseAsync(string address = "localhost", int port = 8080)
        {

            _httpClient?.Dispose();
            _httpClient = new HttpClient();

            // Configure request with proper headers
            using var request = new HttpRequestMessage(HttpMethod.Get, $"http://{address}:{port}/sse");

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

            //EventSource eventSource = new EventSource($"http://{address}:{port}/sse");
            //eventSource.MessageReceived += (sender, e) => Console.WriteLine(e.Message.Data);

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            response.EnsureSuccessStatusCode();

            _stream = await response.Content.ReadAsStreamAsync();

            _rpc?.Dispose();

            _rpc = new JsonRpc(new NewLineDelimitedMessageHandler(
                new DuplexPipe(_stream, new HttpPostStream($"http://{address}:{port}/messages")),
                new SystemTextJsonFormatter()));

            _rpc.StartListening();
            _ = _rpc.InvokeAsync<InitializeResult>(
                 "initialize",
                 [
                     "2024-11-05",
                        new {
                            roots = new { listChanged = false },
                            sampling = new { } },
                        new {
                            name = _name,
                            version = _version }
                 ]);

            _ = _rpc.NotifyAsync("notifications/initialized");
            Initialized = true;
        }

        /// <summary>
        /// Gets a list of tools from the MCP server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of tools.</returns>
        public async Task<List<Tool>> GetToolsAsync()
        {
            Tools = (await _rpc.InvokeWithParameterObjectAsync<ToolsListResult>("tools/list")).Tools;
            return Tools;
        }
        
        /// <summary>
        /// Calls a tool with the given name and parameters.
        /// </summary>
        /// <param name="name">The name of the tool to call.</param>
        /// <param name="parameters">The parameters to pass to the tool.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the result of the tool call.</returns>
        public async Task<CallToolResult> CallToolAsync(string name, Dictionary<string, object> parameters) =>
            await _rpc.InvokeWithParameterObjectAsync<CallToolResult>("tools/call", new ToolCallParameters { Arguments = parameters, Name = name });

        /// <summary>
        /// Calls a tool with the given name.
        /// </summary>
        /// <param name="name">The name of the tool to call.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the result of the tool call.</returns>
        public async Task<CallToolResult> CallToolAsync(string name) => await CallToolAsync(name, []);

        /// <summary>
        /// Gets a list of resources from the MCP server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of resources.</returns>
        public async Task<ResourcesListResult> GetResourcesAsync() => await _rpc.InvokeWithParameterObjectAsync<ResourcesListResult>("resources/list");

        /// <summary>
        /// Gets a list of resource templates from the MCP server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of resource templates.</returns>
        public async Task<ResourceTemplateListResult> GetResourceTemplatesAsync() => await _rpc.InvokeWithParameterObjectAsync<ResourceTemplateListResult>("resources/templates/list");

        /// <summary>
        /// Pings the MCP server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the ping response.</returns>
        public async Task<object> PingAsync() => await _rpc.InvokeWithParameterObjectAsync<object>("ping");

        /// <summary>
        /// Gets a list of prompts from the MCP server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of prompts.</returns>
        public async Task<PromptListResult> GetPromptListAsync() => await _rpc.InvokeWithParameterObjectAsync<PromptListResult>("prompts/list");

        /// <summary>
        /// Releases all resources used by the <see cref="MCPClient"/> class.
        /// </summary>
        public void Dispose()
        {
            _rpc.Dispose();

            _process.Kill();
            _process.WaitForExit();
            _process.Dispose();
        }
    }
}

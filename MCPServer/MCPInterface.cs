using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelContextProtocol
{
    internal class MCPInterface
    {
        private JsonRpc _rpc;

        public MCPInterface()
        {
            // Initialize the JsonRpc server to use standard input/output
            _rpc = new JsonRpc(Console.OpenStandardInput(), Console.OpenStandardOutput(), this);
            _rpc.StartListening();
        }

        // Example method that can be called via JSON-RPC
        [JsonRpcMethod("echo")]
        public string Echo(string message)
        {
            return message;
        }

        // Example method to add two numbers
        [JsonRpcMethod("add")]
        public int Add(int a, int b)
        {
            return a + b;
        }

        public static async Task Main(string[] args)
        {
            // Create an instance of the MCPInterface to start the server
            var server = new MCPInterface();
            Console.WriteLine("MCP server is running...");
            await Task.Delay(-1); // Keep the server running indefinitely
        }
    }
}

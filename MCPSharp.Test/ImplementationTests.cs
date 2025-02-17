using System.Diagnostics;

namespace MCPSharp.Test
{
    [TestClass]
    public sealed class ImplementationTests 
    {
        private static Process? server;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            server = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "MCPSharp.Example.exe",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            server.Start();

            Assert.IsFalse(server.HasExited);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            server!.Kill();
            server.Dispose();
            Console.WriteLine("Server killed");
            Console.WriteLine("Test complete");
        }

        [TestInitialize]
        public void TestInit()
        {
            // This method is called before each test method.
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // This method is called after each test method.
        }

        [TestMethod]
        public async Task TestInitializeAndListTools() 
        {
            Assert.IsNotNull(server, "Server not running!"); 

            await server.StandardInput.WriteLineAsync("{\"jsonrpc\": \"2.0\",\"id\": 1,\"method\": \"initialize\",\"params\": {\"protocolVersion\": \"2024-11-05\",\"capabilities\": {\"roots\": {\"listChanged\": true},\"sampling\": {}},\"clientInfo\": {\"name\": \"ExampleClient\",\"version\": \"1.0.0\"}}}");
            await server.StandardInput.FlushAsync();
            var response = await server.StandardOutput.ReadLineAsync();
            Console.WriteLine(response);
         
            //finish the handshake
            await server.StandardInput.WriteLineAsync("{\"jsonrpc\": \"2.0\",\"method\": \"notifications/initialized\"}");
            await server.StandardInput.FlushAsync();

            //list tools
            await server.StandardInput.WriteLineAsync("{\"jsonrpc\": \"2.0\",\"id\": 1,\"method\": \"tools/list\",\"params\": {}}");
            await server.StandardInput.FlushAsync();
            response = await server.StandardOutput.ReadLineAsync();
            Console.WriteLine(response);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Contains("tool"));
        }
    }
}

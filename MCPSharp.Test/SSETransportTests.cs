namespace MCPSharp.Test
{
    [TestClass()]
    public sealed class SSETransportTests
    {
        private static MCPClient? _client;
        private static MCPClient? client;

        [ClassInitialize]
        [Timeout(5000)]
        public static void ClassInitialize(TestContext context) {
            _client = new("Test Client", "1.0.0", "MCPSharp.Example.exe"); //start the exe
            client = new(new Uri("http://localhost:8000/sse"), "test_sse_client", "1"); //connect to the sse server 
        }


        // [TestMethod("tools/list")] //unucomment this when SSE works
        [Timeout(10000)]
        public async Task TestListToolsAsync() 
        {
            Assert.IsTrue(client!.Initialized);
            var tools = await client.GetToolsAsync();
            Assert.IsNotNull(tools);
            Assert.IsTrue(tools.Count > 0);
            tools.ForEach(tool =>
            {
                Assert.IsFalse(string.IsNullOrEmpty(tool.Name));
                Assert.IsFalse(string.IsNullOrEmpty(tool.Description));
                Console.WriteLine(tool.Name);
            });
        }

      
        [McpTool("test", "testing tool")]
        public class TestTool
        {
            [McpFunction("test", "test function")]
            public async Task<string> TestFunction() => await Task.FromResult("test");
        }

       
    }
}

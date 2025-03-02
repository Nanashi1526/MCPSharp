namespace MCPSharp.Example
{
    ///<summary>testing interface for custom .net mcp server</summary>
    public class MCPDev()
    {
        [McpResource("name", "test://{name}")]
        public string Name(string name) => $"hello {name}";


        [McpResource("settings", "test://settings", "string", "the settings document")]
        public string Settings { get; set; } = "settings";


        [McpTool("write-to-console", "write a string to the console")] 
        public static void WriteToConsole(string message) => Console.WriteLine(message);

        ///<summary>just returns a message for testing.</summary>
        [McpTool] 
        public static string Hello() => "hello, claude.";

        ///<summary>returns ths input string back</summary>
        ///<param name="input">the string to echo</param>
        [McpTool]
        public static string Echo([McpParameter(true)] string input) => input;

        ///<summary>Add Two Numbers</summary>
        ///<param name="a">first number</param>
        ///<param name="b">second number</param>
        [McpTool] 
        public static string Add(int a, int b) => (a + b).ToString();


        /// <summary>
        /// Adds a complex object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [McpTool]
        public static string AddComplex(ComplicatedObject obj) => $"Name: {obj.Name}, Age: {obj.Age}, Hobbies: {string.Join(", ", obj.Hobbies)}";

        /// <summary>
        /// throws an exception - for ensuring we handle them gracefully
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [McpFunction("throw_exception")] //leaving this one as [McpFunction] for testing purposes
        public static string Exception() => throw new Exception("This is an exception");
    }
}
using MCPSharp;
using MCPSharp.Model;
using MCPSharp.ExternalExample;
using MCPSharp.Model.Schemas;

MCPServer.RegisterTool<ExternalTool>(); 
MCPServer.RegisterTool<SemKerExample>();

MCPServer.AddToolHandler( new Tool() 
{
    Name = "dynamicTool",
    Description = "A Test Tool",
    InputSchema = new InputSchema {
        Type = "object",
        Required = ["input"],
        Properties = new Dictionary<string, ParameterSchema>{
            {"input", new ParameterSchema{Type="string", Description="the input"}},
            {"input2", new ParameterSchema{Type="string", Description="the input2"}}
        }
    }
}, (string input, string input2) => { return $"hello, {input}.\n{input2}"; });


await MCPServer.StartAsync("TestServer", "1.0");

namespace MCPSharp.Example
{
    ///<summary>testing interface for custom .net mcp server</summary>
    public class MCPDev()
    {
        [McpResource("name", "test://{name}")]
        public string Name(string name) => $"hello {name}";


        [McpResource("settings", "test://settings")]
        public string Settings { get; set; } = "settings";


        [McpFunction("write-to-console", "write a string to the console")]
        public static void WriteToConsole(string message) => Console.WriteLine(message);

        ///<summary>just returns a message for testing.</summary>
        [McpFunction]
        public static string Hello() => "hello, claude.";

        ///<summary>returns ths input string back</summary>
        ///<param name="input">the string to echo</param>
        [McpFunction]
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
        [McpFunction]
        public static string AddComplex(ComplicatedObject obj) => $"Name: {obj.Name}, Age: {obj.Age}, Hobbies: {string.Join(", ", obj.Hobbies)}";

        /// <summary>
        /// throws an exception - for ensuring we handle them gracefully
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [McpFunction("throw_exception")]
        public static string Exception() => throw new Exception("This is an exception");
    }

    /// <summary>
    /// A complicated object
    /// </summary>
    public class ComplicatedObject()
    {
        /// <summary>The name of the object</summary>
        public string Name { get; set; } = "";
        /// <summary>The age of the object</summary>
        public int Age { get; set; } = 0;
        /// <summary>The hobbies of the object</summary>
        public string[] Hobbies { get; set; } = [];
    }
}
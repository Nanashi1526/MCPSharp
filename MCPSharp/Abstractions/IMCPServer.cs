namespace MCPSharp.Abstractions
{
    /// <summary>
    /// Interface for MCP Server which provides methods to register tools and start the server.
    /// </summary>
    public interface IMCPServer
    {
        /// <summary>
        /// Starts the MCP server.
        /// </summary>
        abstract static Task StartAsync(string serverName, string version);
    }
}
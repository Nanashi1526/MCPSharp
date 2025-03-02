using MCPSharp.Model.Capabilities;

namespace MCPSharp.Model.Parameters
{
    internal class InitializationParameters
    {
        public required string Version { get; set; }
        public required string ProtocolVersion { get; set; }
        public required ClientCapabilities Capabilities { get; set; }
        public required Implementation ClientInfo { get; set; }
    }
}

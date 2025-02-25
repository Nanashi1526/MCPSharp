using System.IO.Pipelines;

namespace MCPSharp.Core.Transport
{
    internal class DuplexPipe(Stream reader, Stream writer) : IDuplexPipe
    {
        private readonly PipeReader _reader = PipeReader.Create(reader);
        private readonly PipeWriter _writer = PipeWriter.Create(writer);

        public PipeReader Input => _reader;
        public PipeWriter Output => _writer;
    }
}
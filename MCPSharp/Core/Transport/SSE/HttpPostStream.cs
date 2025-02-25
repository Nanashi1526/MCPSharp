namespace MCPSharp.Core.Transport.SSE
{
    /// <summary>
    /// A stream that sends data to an HTTP endpoint using POST requests.
    /// </summary>
    public class HttpPostStream : Stream
    {
        private readonly string _endpoint;
        private readonly MemoryStream _buffer;
        private readonly HttpClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpPostStream"/> class.
        /// </summary>
        /// <param name="endpoint"></param>
        public HttpPostStream(string endpoint)
        {
            _endpoint = endpoint;
            _buffer = new MemoryStream();
            _client = new HttpClient();
        }

        /// <summary>
        /// Gets a value indicating whether the stream supports reading.
        /// </summary>
        public override bool CanRead => false;

        /// <summary>
        /// Gets a value indicating whether the stream supports seeking.
        /// </summary>
        public override bool CanSeek => false;

        /// <summary>
        /// Gets a value indicating whether the stream supports writing.
        /// </summary>
        public override bool CanWrite => true;

        /// <summary>
        /// Gets the length of the stream.
        /// </summary>
        public override long Length => throw new NotSupportedException();

        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Writes data to the stream.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            _buffer.Write(buffer, offset, count);
            if (buffer.Contains((byte)'\n'))
            {
                // Make sure to await the flush
                _ = FlushAsync();
            }
        }

        /// <summary>
        /// Flushes the buffer to the endpoint.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            if (_buffer.Length > 0)
            {
                _buffer.Position = 0;
                var content = await new StreamReader(_buffer).ReadToEndAsync();
                _buffer.SetLength(0);

                using var request = new HttpRequestMessage(HttpMethod.Post, _endpoint)
                {
                    Content = new StringContent(content)
                };
                using var response = await _client.SendAsync(request, cancellationToken);
                //response.EnsureSuccessStatusCode();
            }
        }

        /// <summary>
        /// Flushes the buffer to the endpoint.
        /// </summary>
        public override void Flush() => _ = FlushAsync();

        /// <summary>
        /// Reads data from the stream.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public override int Read(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();

        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        /// <summary>
        /// Sets the length of the stream.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="NotSupportedException"></exception>
        public override void SetLength(long value) =>
            throw new NotSupportedException();

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="HttpPostStream" /> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _buffer.Dispose();
                _client.Dispose();
            }
            base.Dispose(disposing);
        }
    }

}

using Microsoft.AspNetCore.Http;
using System.IO.Pipelines;

namespace MCPSharp.Core.Transport.SSE
{
    public class SseClient
    {
        private readonly HttpContext _context;
        private readonly CancellationTokenSource _disconnectTokenSource;
        public Stream Stream;
        public SseClient(HttpContext context, Stream stream)
        { 
            _context = context;
            _disconnectTokenSource = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
            Stream = stream;
        }

        public async Task SendEventAsync(string eventType, string data)
        {
            try
            {
                var response = _context.Response;
                await response.WriteAsync($"event: {eventType}\n");
                await response.WriteAsync($"data: {data}\n\n");
                await response.Body.FlushAsync();
            }
            catch
            {
                _disconnectTokenSource.Cancel();
            }
        }

        public Task WaitForDisconnectAsync()
        {
            return Task.Delay(Timeout.Infinite, _disconnectTokenSource.Token);
        }      
    }
}
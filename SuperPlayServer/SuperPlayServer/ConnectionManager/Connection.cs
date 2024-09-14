using Microsoft.Extensions.Logging;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperPlayServer.ConnectionManager
{
    public class Connection : IConnection
    {
        private readonly ILogger<Connection> _logger;

        public Connection(ILogger<Connection> logger)
        {
            _logger = logger;
        }

        public async Task SendMessage(WebSocket ws, string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);

            await ws.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);

            _logger.LogInformation($"Message = {message} sent successfully");
        }
    }
}
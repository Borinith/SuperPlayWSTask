using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperPlayServer.ConnectionManager
{
    public class Connection : IConnection
    {
        public async Task SendMessage(WebSocket ws, string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);

            var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);

            if (ws.State == WebSocketState.Open)
            {
                await ws.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                // todo log
            }
            else if (ws.State == WebSocketState.Closed || ws.State == WebSocketState.Aborted)
            {
                // todo log
                throw new Exception("Web socket connection is not open");
            }
        }
    }
}
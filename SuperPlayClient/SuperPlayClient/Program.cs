using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperPlayClient
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            using (var ws = new ClientWebSocket())
            {
                // todo log
                await ws.ConnectAsync(new Uri("wss://localhost:7182/ws"), CancellationToken.None);
                // todo log

                var buffer = new byte[1024 * 4];

                while (true)
                {
                    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        // todo log
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    Console.Write(message + "\r");
                    // todo log
                }
            }
        }
    }
}
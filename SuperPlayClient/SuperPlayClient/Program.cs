using System;
using System.Net.WebSockets;
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
                var deviceId = new Guid("B69DADDA-1502-4621-A18D-6188C6D8301C");

                await ws.ConnectAsync(new Uri($"wss://localhost:7182/ws/login/{deviceId}"), CancellationToken.None);
                // todo log

                var bufferGuid = new byte[16]; // Guid size

                var result = await ws.ReceiveAsync(new ArraySegment<byte>(bufferGuid), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    // todo log
                    return;
                }

                var message = new Guid(bufferGuid);

                Console.Write(message);
                // todo log
            }
        }
    }
}
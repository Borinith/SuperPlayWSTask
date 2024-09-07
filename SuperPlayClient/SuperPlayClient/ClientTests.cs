using SuperPlayClient.DTOs;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace SuperPlayClient
{
    public class ClientTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ClientTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task LoginTest()
        {
            using (var ws = new ClientWebSocket())
            {
                // todo log
                Guid? deviceId = new Guid("B69DADDA-1502-4621-A18D-6188C6D8301C");

                await ws.ConnectAsync(new Uri($"wss://localhost:7182/ws/login/{deviceId}"), CancellationToken.None);
                // todo log

                var bufferGuid = new byte[1024 * 4];

                var result = await ws.ReceiveAsync(new ArraySegment<byte>(bufferGuid), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

                    // todo log
                    return;
                }

                var rawMessage = Encoding.UTF8.GetString(bufferGuid).TrimEnd('\0');
                var message = JsonSerializer.Deserialize<DeviceDTO>(rawMessage);

                if (message is not null)
                {
                    if (message.IsOnline)
                    {
                        _testOutputHelper.WriteLine("Player is online now!");
                    }

                    _testOutputHelper.WriteLine(message.PlayerId.ToString());
                    // todo log
                }
            }
        }
    }
}
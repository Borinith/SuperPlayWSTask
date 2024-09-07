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

                var message = await ReceiveMessage(ws);
                var device = JsonSerializer.Deserialize<DeviceDTO>(message);

                if (device is not null)
                {
                    if (device.IsOnline)
                    {
                        _testOutputHelper.WriteLine("Player is online now!");
                    }

                    _testOutputHelper.WriteLine(device.PlayerId.ToString());
                    // todo log
                }
            }
        }

        [Fact]
        public async Task UpdateResourcesTest()
        {
            using (var ws = new ClientWebSocket())
            {
                // todo log
                Guid? deviceId = new Guid("B69DADDA-1502-4621-A18D-6188C6D8301C");
                const ResourceType resourceType = ResourceType.Coins;
                const int value = 50;

                await ws.ConnectAsync(new Uri($"wss://localhost:7182/ws/updateResources/{deviceId}/{resourceType}/{value}"), CancellationToken.None);
                // todo log

                var message = await ReceiveMessage(ws);

                _testOutputHelper.WriteLine($"New value = {message}");
                // todo log
            }
        }

        [Fact]
        public async Task SendGiftTest()
        {
            using (var ws = new ClientWebSocket())
            {
                // todo log
                Guid? deviceId = new Guid("B69DADDA-1502-4621-A18D-6188C6D8301C");
                Guid? friendPlayerId = new Guid("4C8B1953-1A2A-49AB-BA8B-8B0F276412A1");
                const ResourceType resourceType = ResourceType.Coins;
                const int value = 2;

                await ws.ConnectAsync(new Uri($"wss://localhost:7182/ws/sendGift/{deviceId}/{friendPlayerId}/{resourceType}/{value}"), CancellationToken.None);
                // todo log

                var message = await ReceiveMessage(ws);
                var friendDevice = JsonSerializer.Deserialize<DeviceDTO>(message);

                if (friendDevice is not null)
                {
                    if (friendDevice.IsOnline)
                    {
                        _testOutputHelper.WriteLine("Your friend received a gift!");
                    }

                    // todo log
                }
            }
        }

        private static async Task<string> ReceiveMessage(WebSocket ws)
        {
            var bufferGuid = new byte[1024 * 4];

            var receiveResult = await ws.ReceiveAsync(new ArraySegment<byte>(bufferGuid), CancellationToken.None);

            if (receiveResult.MessageType == WebSocketMessageType.Close)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

                // todo log
                return string.Empty;
            }

            return Encoding.UTF8.GetString(bufferGuid).TrimEnd('\0');
        }
    }
}
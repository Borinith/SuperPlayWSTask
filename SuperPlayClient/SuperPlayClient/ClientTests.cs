using Microsoft.Extensions.Configuration;
using Serilog;
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
        private readonly ILogger _logger;
        private readonly ITestOutputHelper _testOutputHelper;

        public ClientTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;

            _logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.TestOutput(testOutputHelper)
                .CreateLogger();
        }

        private static IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            return config;
        }

        [Fact]
        public async Task LoginTest()
        {
            var config = InitConfiguration();
            var deviceId = new Guid(config["DeviceId"]!);

            _logger.Information("Init configuration");

            var sendData = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new MessageDTO(Method.Login, deviceId.ToString())));

            using var ws = new ClientWebSocket();
            await ws.ConnectAsync(new Uri(config["Uri"]!), CancellationToken.None);

            while (ws.State == WebSocketState.Open)
            {
                await ws.SendAsync(new ArraySegment<byte>(sendData), WebSocketMessageType.Text, false, CancellationToken.None);

                _logger.Information($"Run login test with deviceId={deviceId}");

                var message = await ProcessMessage(ws);
                var device = JsonSerializer.Deserialize<DeviceDTO>(message);

                if (device is not null)
                {
                    if (device.IsOnline)
                    {
                        WriteLog("Player is online now!");
                    }

                    WriteLog(device.PlayerId.ToString());
                }

                // Close WS for testing
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

                WriteLog("Connection closed");
            }
        }

        [Fact]
        public async Task UpdateResourcesTest()
        {
            var config = InitConfiguration();
            var deviceId = new Guid(config["DeviceId"]!);

            _logger.Information("Init configuration");

            const ResourceType resourceType = ResourceType.Coins;
            const int value = 50;

            var sendData = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(
                    new MessageDTO(
                        Method.UpdateResources,
                        JsonSerializer.Serialize(new UpdateResourcesDTO(deviceId, resourceType, value)
                        ))));

            using var ws = new ClientWebSocket();
            await ws.ConnectAsync(new Uri(config["Uri"]!), CancellationToken.None);

            while (ws.State == WebSocketState.Open)
            {
                await ws.SendAsync(new ArraySegment<byte>(sendData), WebSocketMessageType.Text, false, CancellationToken.None);

                _logger.Information($"Run update resources test with deviceId={deviceId}, resourceType={resourceType}, value={value}");

                var message = await ProcessMessage(ws);

                WriteLog($"New value = {message}");

                // Close WS for testing
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

                WriteLog("Connection closed");
            }
        }

        [Fact]
        public async Task SendGiftTest()
        {
            var config = InitConfiguration();
            var deviceId = new Guid(config["DeviceId"]!);
            var friendPlayerId = new Guid(config["Friends:0:FriendId"]!);

            _logger.Information("Init configuration");

            const ResourceType resourceType = ResourceType.Coins;
            const int value = 2;

            var sendData = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(
                    new MessageDTO(
                        Method.SendGift,
                        JsonSerializer.Serialize(new SendGiftDTO(deviceId, friendPlayerId, resourceType, value)
                        ))));

            using var ws = new ClientWebSocket();
            await ws.ConnectAsync(new Uri(config["Uri"]!), CancellationToken.None);

            while (ws.State == WebSocketState.Open)
            {
                await ws.SendAsync(new ArraySegment<byte>(sendData), WebSocketMessageType.Text, false, CancellationToken.None);

                _logger.Information($"Run send gift test with deviceId={deviceId}, friendPlayerId={friendPlayerId}, resourceType={resourceType}, value={value}");

                var message = await ProcessMessage(ws);
                var friendDevice = JsonSerializer.Deserialize<DeviceDTO>(message);

                if (friendDevice is not null && friendDevice.IsOnline)
                {
                    WriteLog("Your friend received a gift!");
                }

                // Close WS for testing
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

                WriteLog("Connection closed");
            }
        }

        private async Task<string> ProcessMessage(WebSocket ws)
        {
            var bufferGuid = new byte[1024 * 4];

            var receiveResult = await ws.ReceiveAsync(new ArraySegment<byte>(bufferGuid), CancellationToken.None);

            if (receiveResult.MessageType == WebSocketMessageType.Close)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

                _logger.Error("Message type is close");

                return string.Empty;
            }

            return Encoding.UTF8.GetString(bufferGuid).TrimEnd('\0');
        }

        private void WriteLog(string message)
        {
            _testOutputHelper.WriteLine(message);
            _logger.Information(message);
        }
    }
}
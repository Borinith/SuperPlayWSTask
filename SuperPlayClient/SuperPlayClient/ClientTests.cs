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
            using (var ws = new ClientWebSocket())
            {
                var config = InitConfiguration();
                Guid? deviceId = new Guid(config["DeviceId"]!);

                _logger.Information("Init configuration");

                await ws.ConnectAsync(new Uri(config["Uri"] + $"login/{deviceId}"), CancellationToken.None);

                _logger.Information($"Run login test with deviceId={deviceId}");

                var message = await ReceiveMessage(ws);
                var device = JsonSerializer.Deserialize<DeviceDTO>(message);

                if (device is not null)
                {
                    if (device.IsOnline)
                    {
                        WriteLog("Player is online now!");
                    }

                    WriteLog(device.PlayerId.ToString());
                }
            }
        }

        [Fact]
        public async Task UpdateResourcesTest()
        {
            using (var ws = new ClientWebSocket())
            {
                var config = InitConfiguration();
                Guid? deviceId = new Guid(config["DeviceId"]!);

                _logger.Information("Init configuration");

                const ResourceType resourceType = ResourceType.Coins;
                const int value = 50;

                await ws.ConnectAsync(new Uri(config["Uri"] + $"updateResources/{deviceId}/{resourceType}/{value}"), CancellationToken.None);

                _logger.Information($"Run update resources test with deviceId={deviceId}, resourceType={resourceType}, value={value}");

                var message = await ReceiveMessage(ws);

                WriteLog($"New value = {message}");
            }
        }

        [Fact]
        public async Task SendGiftTest()
        {
            using (var ws = new ClientWebSocket())
            {
                var config = InitConfiguration();
                Guid? deviceId = new Guid(config["DeviceId"]!);
                Guid? friendPlayerId = new Guid(config["Friends:0:FriendId"]!);

                _logger.Information("Init configuration");

                const ResourceType resourceType = ResourceType.Coins;
                const int value = 2;

                await ws.ConnectAsync(new Uri(config["Uri"] + $"sendGift/{deviceId}/{friendPlayerId}/{resourceType}/{value}"), CancellationToken.None);

                _logger.Information($"Run send gift test with deviceId={deviceId}, friendPlayerId={friendPlayerId}, resourceType={resourceType}, value={value}");

                var message = await ReceiveMessage(ws);
                var friendDevice = JsonSerializer.Deserialize<DeviceDTO>(message);

                if (friendDevice is not null && friendDevice.IsOnline)
                {
                    WriteLog("Your friend received a gift!");
                }
            }
        }

        private async Task<string> ReceiveMessage(WebSocket ws)
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
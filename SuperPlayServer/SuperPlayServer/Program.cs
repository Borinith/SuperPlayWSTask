using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SuperPlayServer.ConnectionManager;
using SuperPlayServer.Controllers;
using SuperPlayServer.Data;
using SuperPlayServer.DTOs;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("https://localhost:7182/");

builder.Services.AddDbContext<SuperplayContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers().AddControllersAsServices();
builder.Services.AddScoped<IConnection, Connection>();
builder.Services.AddScoped<LoginController>();
builder.Services.AddScoped<SendGiftController>();
builder.Services.AddScoped<UpdateResourcesController>();

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2) // WS keep-alive interval
    //ReceiveBufferSize = 4 * 1024 // 4 KB buffer
});

app.Run(async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();

        var buffer = new byte[1024 * 4];

        while (webSocket.State == WebSocketState.Open)
        {
            try
            {
                var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

                    Log.Error("Message type is close");

                    break;
                }

                var messageRaw = Encoding.UTF8.GetString(buffer).TrimEnd('\0');

                var result = await ProcessMessage(messageRaw);
                await SendReplyMessage(webSocket, result);
            }
            catch (WebSocketException ex)
            {
                if (webSocket.State == WebSocketState.Aborted)
                {
                    Log.Warning("WebSocket connection aborted unexpectedly. Handling closure gracefully.");

                    // Exit the loop if the WebSocket is aborted
                    break;
                }

                Log.Error(ex, "Error receiving message from WebSocket.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error in WebSocket processing.");
            }
        }
    }
    else
    {
        // Reject non-ws requests
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});

await app.RunAsync();

return;

async Task<string> ProcessMessage(string messageRaw)
{
    var message = JsonSerializer.Deserialize<Message>(messageRaw);

    // Deserialize the message and route it to the relevant handler, i.e. Login, SendGift, UpdateResource etc...
    if (message is not null && Enum.TryParse<Method>(message.MethodName, out var method))
    {
        switch (method)
        {
            case Method.Login:
            {
                using var scope = app.Services.CreateScope();
                var loginService = scope.ServiceProvider.GetRequiredService<LoginController>();

                return await loginService.Login(new Guid(message.Data));
            }

            case Method.SendGift:
            {
                using var scope = app.Services.CreateScope();
                var sendGiftService = scope.ServiceProvider.GetRequiredService<SendGiftController>();

                return await sendGiftService.SendGift(JsonSerializer.Deserialize<SendGiftDTO>(message.Data)!);
            }

            case Method.UpdateResources:
            {
                using var scope = app.Services.CreateScope();
                var updateResourcesService = scope.ServiceProvider.GetRequiredService<UpdateResourcesController>();

                return await updateResourcesService.UpdateResources(JsonSerializer.Deserialize<UpdateResourcesDTO>(message.Data)!);
            }

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    var errorMessage = $"Unexpected error in deserializing message {messageRaw}";
    Log.Error(errorMessage);

    return errorMessage;
}

async Task SendReplyMessage(WebSocket ws, string message)
{
    using var scope = app.Services.CreateScope();
    var connection = scope.ServiceProvider.GetRequiredService<IConnection>();
    await connection.SendMessage(ws, message);
}
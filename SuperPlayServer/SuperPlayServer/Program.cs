using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("https://localhost:7182/");
var app = builder.Build();

app.MapGet("/", () => "Without sockets\n" + $"{DateTime.Now:dd.MM.yyyy HH:mm:ss}");

app.UseWebSockets();

app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using (var ws = await context.WebSockets.AcceptWebSocketAsync())
        {
            while (true)
            {
                var message = "With sockets\n" + $"{DateTime.Now:dd.MM.yyyy HH:mm:ss}";
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
                    break;
                }

                Thread.Sleep(100);
            }
        }
    }
    else
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        // todo log
    }
});

await app.RunAsync();
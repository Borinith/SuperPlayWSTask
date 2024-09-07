using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperPlayServer.Data;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace SuperPlayServer.Controllers
{
    [Route("/ws/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly SuperplayContext _context;

        public LoginController(SuperplayContext context)
        {
            _context = context;
        }

        [HttpGet("{deviceId}")]
        public async Task Login(Guid deviceId)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using (var ws = await HttpContext.WebSockets.AcceptWebSocketAsync())
                {
                    //await Init();

                    var device = await _context.Devices.AsNoTracking().FirstOrDefaultAsync(x => x.Id == deviceId);

                    var playerId = device?.PlayerId ?? throw new Exception($"Player with device id={deviceId} is not found");

                    var bytes = playerId.ToByteArray();
                    var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);

                    if (ws.State == WebSocketState.Open)
                    {
                        await ws.SendAsync(arraySegment, WebSocketMessageType.Binary, true, CancellationToken.None);
                        // todo log
                    }
                    else if (ws.State == WebSocketState.Closed || ws.State == WebSocketState.Aborted)
                    {
                        // todo log
                        throw new Exception("Web socket connection is not open");
                    }
                }
            }
            else
            {
                // todo log
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

                throw new Exception("It is not a web socket connection");
            }
        }

        private async Task Init()
        {
            _context.Devices.Add(new Device
            {
                Id = new Guid("B69DADDA-1502-4621-A18D-6188C6D8301C"),
                PlayerId = new Guid("F4F30EBC-900F-4B7E-808A-98AF99B2354B"),
                IsOnline = true
            });

            await _context.SaveChangesAsync();
        }
    }
}
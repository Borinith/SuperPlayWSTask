using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperPlayServer.Data;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
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

        [HttpGet("{deviceId?}")]
        public async Task Login(Guid? deviceId)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();

                var device = new Device();

                if (deviceId is null || await _context.Devices.AsNoTracking().FirstOrDefaultAsync(x => x.Id == deviceId.Value) is not { } existingDevice)
                {
                    device.Id = Guid.NewGuid(); //new Guid("B69DADDA-1502-4621-A18D-6188C6D8301C");
                    device.PlayerId = Guid.NewGuid(); //new Guid("F4F30EBC-900F-4B7E-808A-98AF99B2354B");
                    device.IsOnline = true;

                    _context.Devices.Add(device);

                    await _context.SaveChangesAsync();
                }
                else
                {
                    device.Id = existingDevice.Id;
                    device.PlayerId = existingDevice.PlayerId;
                    device.IsOnline = existingDevice.IsOnline;
                }

                var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(device));

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
            else
            {
                // todo log
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

                throw new Exception("It is not a web socket connection");
            }
        }
    }
}
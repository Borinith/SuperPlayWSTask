using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperPlayServer.ConnectionManager;
using SuperPlayServer.Data;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace SuperPlayServer.Controllers
{
    [Route("/ws/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IConnection _connection;
        private readonly SuperplayContext _context;

        public LoginController(IConnection connection, SuperplayContext context)
        {
            _connection = connection;
            _context = context;
        }

        [HttpGet("{deviceId?}")]
        public async Task Login(Guid? deviceId)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
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

                using var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await _connection.SendMessage(ws, JsonSerializer.Serialize(device));
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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperPlayServer.Data;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperPlayServer.Controllers
{
    [Route("/ws/[controller]")]
    public class UpdateResourcesController : ControllerBase
    {
        private readonly SuperplayContext _context;

        public UpdateResourcesController(SuperplayContext context)
        {
            _context = context;
        }

        [HttpGet("{deviceId}/{resourceType}/{value}")]
        public async Task UpdateResources(Guid deviceId, ResourceType resourceType, int value)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var resource = await _context.Resources
                                   .Include(x => x.Device)
                                   .FirstOrDefaultAsync(x =>
                                       x.Device.Id == deviceId && x.ResourceType == resourceType) ??
                               throw new Exception("Resource not found");

                var newResourceValue = resource.ResourceValue + value;

                resource.ResourceValue = newResourceValue;

                await _context.SaveChangesAsync();

                using var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();

                var bytes = Encoding.UTF8.GetBytes(newResourceValue.ToString());

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
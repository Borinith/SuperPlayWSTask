using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperPlayServer.ConnectionManager;
using SuperPlayServer.Data;
using System;
using System.Threading.Tasks;

namespace SuperPlayServer.Controllers
{
    [Route("/ws/[controller]")]
    public class UpdateResourcesController : ControllerBase
    {
        private readonly IConnection _connection;
        private readonly SuperplayContext _context;

        public UpdateResourcesController(IConnection connection, SuperplayContext context)
        {
            _connection = connection;
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
                await _connection.SendMessage(ws, newResourceValue.ToString());
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
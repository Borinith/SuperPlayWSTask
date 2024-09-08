using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<UpdateResourcesController> _logger;

        public UpdateResourcesController(IConnection connection, SuperplayContext context, ILogger<UpdateResourcesController> logger)
        {
            _connection = connection;
            _context = context;
            _logger = logger;
        }

        [HttpGet("{deviceId}/{resourceType}/{value}")]
        public async Task UpdateResources(Guid deviceId, ResourceType resourceType, int value)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var resource = await _context.Resources
                    .Include(x => x.Device)
                    .FirstOrDefaultAsync(x => x.Device.Id == deviceId && x.ResourceType == resourceType);

                if (resource is null)
                {
                    _logger.LogError("Resource not found");

                    return;
                }

                var newResourceValue = resource.ResourceValue + value;

                resource.ResourceValue = newResourceValue;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"New value of resource is {newResourceValue}");

                using var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await _connection.SendMessage(ws, newResourceValue.ToString());
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

                _logger.LogError("It is not a web socket connection");
            }
        }
    }
}
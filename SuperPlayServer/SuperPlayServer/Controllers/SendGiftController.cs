using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SuperPlayServer.ConnectionManager;
using SuperPlayServer.Data;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace SuperPlayServer.Controllers
{
    [Route("/ws/[controller]")]
    public class SendGiftController : ControllerBase
    {
        private readonly IConnection _connection;
        private readonly SuperplayContext _context;
        private readonly ILogger<SendGiftController> _logger;

        public SendGiftController(IConnection connection, SuperplayContext context, ILogger<SendGiftController> logger)
        {
            _connection = connection;
            _context = context;
            _logger = logger;
        }

        [HttpGet("{deviceId}/{friendPlayerId}/{resourceType}/{value}")]
        public async Task SendGift(Guid deviceId, Guid friendPlayerId, ResourceType resourceType, int value)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                if (value <= 0)
                {
                    _logger.LogError($"It is not possible to send a gift, because value is {value}!");

                    return;
                }

                var resourceCurrentPlayer = await _context.Resources
                    .Include(x => x.Device)
                    .FirstOrDefaultAsync(x => x.Device.Id == deviceId && x.ResourceType == resourceType);

                if (resourceCurrentPlayer is null)
                {
                    _logger.LogError($"Resource of current player with device id={deviceId} not found");

                    return;
                }

                if (resourceCurrentPlayer.ResourceValue < value)
                {
                    _logger.LogError($"It is not possible to send a gift because value={value} is greater than resource value={resourceCurrentPlayer.ResourceValue} of current player!");

                    return;
                }

                var resourceFriend = await _context.Resources
                    .Include(x => x.Device)
                    .FirstOrDefaultAsync(x => x.Device.PlayerId == friendPlayerId && x.ResourceType == resourceType);

                if (resourceFriend is null)
                {
                    _logger.LogError($"Resource of friend with id={friendPlayerId} not found");

                    return;
                }

                resourceCurrentPlayer.ResourceValue -= value;
                resourceFriend.ResourceValue += value;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Your resources with type {resourceType.ToString()} have decreased by {value}");
                _logger.LogInformation($"Resources of your friend with id {friendPlayerId} and with type {resourceType.ToString()} have increased by {value}");

                using var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await _connection.SendMessage(ws, JsonSerializer.Serialize(resourceFriend.Device));
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

                _logger.LogError("It is not a web socket connection");
            }
        }
    }
}
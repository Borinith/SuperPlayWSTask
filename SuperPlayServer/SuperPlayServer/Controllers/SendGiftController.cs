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
    public class SendGiftController : ControllerBase
    {
        private readonly IConnection _connection;
        private readonly SuperplayContext _context;

        public SendGiftController(IConnection connection, SuperplayContext context)
        {
            _connection = connection;
            _context = context;
        }

        [HttpGet("{deviceId}/{friendPlayerId}/{resourceType}/{value}")]
        public async Task SendGift(Guid deviceId, Guid friendPlayerId, ResourceType resourceType, int value)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                if (value <= 0)
                {
                    // todo log
                    throw new Exception("It is not possible to send a gift!");
                }

                var resourceCurrentPlayer = await _context.Resources
                                                .Include(x => x.Device)
                                                .FirstOrDefaultAsync(x =>
                                                    x.Device.Id == deviceId && x.ResourceType == resourceType) ??
                                            throw new Exception("Resource of current player not found");

                if (resourceCurrentPlayer.ResourceValue < value)
                {
                    // todo log
                    throw new Exception("It is not possible to send a gift!");
                }

                var resourceFriend = await _context.Resources
                                         .Include(x => x.Device)
                                         .FirstOrDefaultAsync(x =>
                                             x.Device.PlayerId == friendPlayerId && x.ResourceType == resourceType) ??
                                     throw new Exception("Resource of friend not found");

                resourceCurrentPlayer.ResourceValue -= value;
                resourceFriend.ResourceValue += value;

                await _context.SaveChangesAsync();

                using var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await _connection.SendMessage(ws, JsonSerializer.Serialize(resourceFriend.Device));
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
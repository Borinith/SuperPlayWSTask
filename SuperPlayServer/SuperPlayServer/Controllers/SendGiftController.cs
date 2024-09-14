using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SuperPlayServer.Data;
using SuperPlayServer.DTOs;
using System.Text.Json;
using System.Threading.Tasks;

namespace SuperPlayServer.Controllers
{
    public class SendGiftController
    {
        private readonly SuperplayContext _context;
        private readonly ILogger<SendGiftController> _logger;

        public SendGiftController(SuperplayContext context, ILogger<SendGiftController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> SendGift(SendGiftDTO sendGift)
        {
            if (sendGift.Value <= 0)
            {
                var errorMessage = $"It is not possible to send a gift, because value is {sendGift.Value}!";
                _logger.LogError(errorMessage);

                return errorMessage;
            }

            var resourceCurrentPlayer = await _context.Resources
                .Include(x => x.Device)
                .FirstOrDefaultAsync(x => x.Device.Id == sendGift.DeviceId && x.ResourceType == sendGift.ResourceType);

            if (resourceCurrentPlayer is null)
            {
                var errorMessage = $"Resource of current player with device id={sendGift.DeviceId} not found";
                _logger.LogError(errorMessage);

                return errorMessage;
            }

            if (resourceCurrentPlayer.ResourceValue < sendGift.Value)
            {
                var errorMessage = $"It is not possible to send a gift because value={sendGift.Value} is greater than resource value={resourceCurrentPlayer.ResourceValue} of current player!";
                _logger.LogError(errorMessage);

                return errorMessage;
            }

            var resourceFriend = await _context.Resources
                .Include(x => x.Device)
                .FirstOrDefaultAsync(x => x.Device.PlayerId == sendGift.FriendPlayerId && x.ResourceType == sendGift.ResourceType);

            if (resourceFriend is null)
            {
                var errorMessage = $"Resource of friend with id={sendGift.FriendPlayerId} not found";
                _logger.LogError(errorMessage);

                return errorMessage;
            }

            resourceCurrentPlayer.ResourceValue -= sendGift.Value;
            resourceFriend.ResourceValue += sendGift.Value;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Your resources with type {sendGift.ResourceType.ToString()} have decreased by {sendGift.Value}");
            _logger.LogInformation($"Resources of your friend with id {sendGift.FriendPlayerId} and with type {sendGift.ResourceType.ToString()} have increased by {sendGift.Value}");

            return JsonSerializer.Serialize(resourceFriend.Device);
        }
    }
}
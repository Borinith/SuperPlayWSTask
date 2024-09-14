using System;

namespace SuperPlayClient.DTOs
{
    public record SendGiftDTO(Guid DeviceId, Guid FriendPlayerId, ResourceType ResourceType, int Value);
}
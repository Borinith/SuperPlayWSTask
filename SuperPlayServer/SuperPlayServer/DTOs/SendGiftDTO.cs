using SuperPlayServer.Data;
using System;

namespace SuperPlayServer.DTOs
{
    public record SendGiftDTO(Guid DeviceId, Guid FriendPlayerId, ResourceType ResourceType, int Value);
}
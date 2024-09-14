using SuperPlayServer.Data;
using System;

namespace SuperPlayServer.DTOs
{
    public record UpdateResourcesDTO(Guid DeviceId, ResourceType ResourceType, int Value);
}
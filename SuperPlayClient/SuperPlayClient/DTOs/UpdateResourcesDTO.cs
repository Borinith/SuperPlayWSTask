using System;

namespace SuperPlayClient.DTOs
{
    public record UpdateResourcesDTO(Guid DeviceId, ResourceType ResourceType, int Value);
}
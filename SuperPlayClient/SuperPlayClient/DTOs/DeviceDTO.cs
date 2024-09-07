using System;

namespace SuperPlayClient.DTOs
{
    public record DeviceDTO(Guid Id, Guid PlayerId, bool IsOnline);
}
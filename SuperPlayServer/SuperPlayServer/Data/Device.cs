using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace SuperPlayServer.Data
{
    [Index(nameof(PlayerId))]
    public class Device
    {
        [Key]
        public Guid Id { get; set; }

        public Guid PlayerId { get; set; }

        public bool IsOnline { get; set; }
    }
}
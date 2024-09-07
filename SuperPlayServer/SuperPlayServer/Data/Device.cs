using System;
using System.ComponentModel.DataAnnotations;

namespace SuperPlayServer.Data
{
    public class Device
    {
        [Key]
        public Guid Id { get; set; }

        public Guid PlayerId { get; set; }

        public bool IsOnline { get; set; }
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperPlayServer.Data
{
    public class Resource
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public ResourceType ResourceType { get; set; }

        public int ResourceValue { get; set; }

        public Guid DeviceId { get; set; }

        public virtual Device Device { get; set; } = null!;
    }
}
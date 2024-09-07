using Microsoft.EntityFrameworkCore;

namespace SuperPlayServer.Data
{
    public class SuperplayContext : DbContext
    {
        public SuperplayContext(DbContextOptions<SuperplayContext> options) : base(options)
        {
        }

        public DbSet<Device> Devices { get; set; }

        public DbSet<Resource> Resources { get; set; }
    }
}
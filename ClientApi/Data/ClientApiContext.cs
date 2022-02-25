using Microsoft.EntityFrameworkCore;
using ClientApi.Models;

namespace ClientApi.Data
{
    public class ClientApiContext : DbContext
    {
        public ClientApiContext(DbContextOptions<ClientApiContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
    }
}

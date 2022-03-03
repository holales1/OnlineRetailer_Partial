using Microsoft.EntityFrameworkCore;
using EmailApi.Models;

namespace EmailApi.Data
{
    public class EmailApiContext : DbContext
    {
        public EmailApiContext(DbContextOptions<EmailApiContext> options)
            : base(options)
        {
        }

        public DbSet<Email> Emails { get; set; }
    }
}

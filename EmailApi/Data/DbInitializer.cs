using System.Collections.Generic;
using System.Linq;
using EmailApi.Models;

namespace EmailApi.Data
{
    public class DbInitializer : IDbInitializer
    {
        // This method will create and seed the database.
        public void Initialize(EmailApiContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Look for any Emails
            if (context.Emails.Any())
            {
                return;   // DB has been seeded
            }

            List<Email> emails = new List<Email>
            {
                new Email { Id = 1, Destination = "pedro@gmail.com", Content = "Welcome" },

                new Email { Id = 2, Destination = "paco@gmail.com", Content = "Welcome" }

            };

            context.Emails.AddRange(emails);
            context.SaveChanges();
        }
    }
}

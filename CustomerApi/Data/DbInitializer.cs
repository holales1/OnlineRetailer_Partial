using System.Collections.Generic;
using System.Linq;
using CustomerApi.Models;
using System;

namespace CustomerApi.Data
{
    public class DbInitializer : IDbInitializer
    {
        // This method will create and seed the database.
        public void Initialize(CustomerApiContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Look for any Products
            if (context.Customers.Any())
            {
                return;   // DB has been seeded
            }

            List<Customer> customers = new List<Customer>
            {
                new Customer { Id = 1,
                               Name = "Pedro",
                               Email = "pedro@gmail.com",
                               Phone = "+4500000001",
                               BillingAddress = "Fake street 123, 6500 Esbjerg",
                               ShippingAddress = "Fake street 123, 6500 Esbjerg",
                               CreditStanding = 270 },

                new Customer { Id = 2,
                                Name = "Paco",
                                Email = "paco@gmail.com",
                                Phone = "+4500000002",
                                BillingAddress = "Pidgeon street 1, 6500 Esbjerg",
                                ShippingAddress = "Pidgeon street 1, 6500 Esbjerg",
                                CreditStanding = 0 }
            };

            context.Customers.AddRange(customers);
            context.SaveChanges();
        }
    }
}

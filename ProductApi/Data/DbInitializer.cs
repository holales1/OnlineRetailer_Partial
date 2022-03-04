using System.Collections.Generic;
using System.Linq;
using ProductApi.Models;

namespace ProductApi.Data
{
    public class DbInitializer : IDbInitializer
    {
        // This method will create and seed the database.
        public void Initialize(ProductApiContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Look for any Products
            if (context.Products.Any())
            {
                return;   // DB has been seeded
            }

            List<Product> products = new List<Product>
            {
                new Product { Name = "Hammer", Price = 100, Category = "Tools", ItemsInStock = 10, ItemsReserved = 2 },
                new Product { Name = "Screwdriver", Price = 70, Category = "Tools", ItemsInStock = 20, ItemsReserved = 1 },
                new Product { Name = "Drill", Price = 500, Category = "Tools", ItemsInStock = 2, ItemsReserved = 0 }
            };

            context.Products.AddRange(products);
            context.SaveChanges();
        }
    }
}

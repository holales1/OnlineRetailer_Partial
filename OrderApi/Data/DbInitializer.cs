using System.Collections.Generic;
using System.Linq;
using OrderApi.Models;
using System;

namespace OrderApi.Data
{
    public class DbInitializer : IDbInitializer
    {
        // This method will create and seed the database.
        public void Initialize(OrderApiContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Look for any Products
            if (context.Orders.Any())
            {
                return;   // DB has been seeded
            }

            List<ProductQuantity> productQuantities = new List<ProductQuantity>();
            productQuantities.Add(new ProductQuantity() { ProductId =1, Quantity=2});

            List<ProductQuantity> productQuantities2 = new List<ProductQuantity>();
            productQuantities2.Add(new ProductQuantity() { ProductId = 1, Quantity = 2 });

            List<Order> orders = new List<Order>
            {
                new Order { Date = DateTime.Today, ProductId = 1, Quantity = 2, State = Order.Status.Paid, CustomerId = 2, ProductList = productQuantities },
                new Order { Date = DateTime.Today, ProductId = 1, Quantity = 2, State = Order.Status.Completed, CustomerId = 1, ProductList = productQuantities2 },

            };

            context.Orders.AddRange(orders);
            context.SaveChanges();
        }
    }
}

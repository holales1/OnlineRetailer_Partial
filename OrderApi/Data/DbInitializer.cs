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
            if (context.Orders.Any() || context.OrderLines.Any())
            {
                return;   // DB has been seeded
            }

            List<Order> orders = new List<Order>
            {
                new Order { Date = DateTime.Today, State = Order.Status.Paid, CustomerId = 2 },
                new Order { Date = DateTime.Today, State = Order.Status.Completed, CustomerId = 1 },

            };

            List<OrderLine> orderLines = new List<OrderLine> 
            { 
                new OrderLine{ OrderId=1, ProductId=1,Quantity=2},
                new OrderLine{ OrderId=1, ProductId=2,Quantity=1},
                new OrderLine{ OrderId=1, ProductId=3,Quantity=1},

                new OrderLine{ OrderId=2, ProductId=1,Quantity=2},
                new OrderLine{ OrderId=2, ProductId=2,Quantity=1},
            };

            context.Orders.AddRange(orders);
            context.OrderLines.AddRange(orderLines);
            context.SaveChanges();
        }
    }
}

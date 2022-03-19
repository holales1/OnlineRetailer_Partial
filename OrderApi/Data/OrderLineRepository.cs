using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using OrderApi.Models;
using System;
using SharedModels;

namespace OrderApi.Data
{
    public class OrderLineRepository : IRepositoryOrderLine<OrderLine>
    {
        private readonly OrderApiContext db;

        public OrderLineRepository(OrderApiContext context)
        {
            db = context;
        }

        OrderLine IRepositoryOrderLine<OrderLine>.Add(OrderLine entity)
        {
            
            var newOrderLine = db.OrderLines.Add(entity).Entity;
            db.SaveChanges();
            return newOrderLine;
        }

        IEnumerable<OrderLine> IRepositoryOrderLine<OrderLine>.GetByOrderId(int orderId)
        {
            return db.OrderLines.Where(o => o.OrderId == orderId).ToList();
        }
    }
}

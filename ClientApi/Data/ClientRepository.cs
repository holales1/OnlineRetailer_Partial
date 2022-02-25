using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using ClientApi.Models;
using System;

namespace ClientApi.Data
{
    public class ClientRepository : IRepository<Order>
    {
        private readonly ClientApiContext db;

        public ClientRepository(ClientApiContext context)
        {
            db = context;
        }

        Order IRepository<Order>.Add(Order entity)
        {
            if (entity.Date == null)
                entity.Date = DateTime.Now;
            
            var newOrder = db.Orders.Add(entity).Entity;
            db.SaveChanges();
            return newOrder;
        }

        void IRepository<Order>.Edit(Order entity)
        {
            db.Entry(entity).State = EntityState.Modified;
            db.SaveChanges();
        }

        Order IRepository<Order>.Get(int id)
        {
            return db.Orders.FirstOrDefault(o => o.Id == id);
        }

        IEnumerable<Order> IRepository<Order>.GetAll()
        {
            return db.Orders.ToList();
        }

        void IRepository<Order>.Remove(int id)
        {
            var order = db.Orders.FirstOrDefault(p => p.Id == id);
            db.Orders.Remove(order);
            db.SaveChanges();
        }
    }
}

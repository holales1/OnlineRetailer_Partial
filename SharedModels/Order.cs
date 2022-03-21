using System;
using System.Collections.Generic;

namespace SharedModels
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime? Date { get; set; }
        public ICollection<OrderLine> OrderLines { get; set; } = new HashSet<OrderLine>();
        public int CustomerId { get; set; }
        public Status State { get; set; }

        public enum Status
        {
            Completed,
            Cancelled,
            Shipped,
            Paid
        }
    }
}

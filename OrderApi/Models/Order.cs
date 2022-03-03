using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderApi.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime? Date { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public List<ProductQuantity> ProductList { get; set; } = new List<ProductQuantity>();
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

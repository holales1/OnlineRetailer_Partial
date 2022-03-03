using Microsoft.EntityFrameworkCore;
using System;
namespace OrderApi.Models
{
    [Keyless]
    public class ProductQuantity
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}

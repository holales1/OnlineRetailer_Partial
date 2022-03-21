using System.Collections.Generic;

namespace SharedModels
{
    public class OrderStatusChangedMessage
    {
        public int? CustomerId { get; set; }
        public ICollection<OrderLine> OrderLines { get; set; }
        public int Amount { get; set; }
    }
}

using SharedModels;
using System.Collections.Generic;

namespace OrderApi.Infrastructure
{
    public interface IMessagePublisher
    {
        void PublishOrderStatusChangedMessage(int? customerId,
                                              ICollection<OrderLine> orderLines,
                                              int amount,
                                              string topic);
        void PublishOrderStatusChangedMessage(int? customerId,
                                              ICollection<OrderLine> orderLines,
                                              string topic);
    }
}

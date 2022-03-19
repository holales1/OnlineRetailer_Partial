using EasyNetQ;
using SharedModels;
using System;
using System.Collections.Generic;

namespace OrderApi.Infrastructure
{
    public class MessagePublisher : IMessagePublisher, IDisposable
    {
        IBus bus;

        public MessagePublisher(string connectionString)
        {
            bus = RabbitHutch.CreateBus(connectionString);
        }


        public void Dispose()
        {
            bus.Dispose();
        }

        public void PublishOrderStatusChangedMessage(int? customerId, ICollection<OrderLine> orderLines, int amount, string topic)
        {
            OrderStatusChangedMessage message = new OrderStatusChangedMessage
            {
                CustomerId = customerId,
                OrderLines = orderLines,
                Amount = amount
            };

            bus.PubSub.Publish(message, topic);
        }

        public void PublishOrderStatusChangedMessage(int? customerId, ICollection<OrderLine> orderLines, string topic)
        {
            OrderStatusChangedMessage message = new OrderStatusChangedMessage
            {
                CustomerId = customerId,
                OrderLines = orderLines
            };

            bus.PubSub.Publish(message, topic);
        }
    }
}

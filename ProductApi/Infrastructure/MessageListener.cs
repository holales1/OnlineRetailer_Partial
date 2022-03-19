using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;
using ProductApi.Data;
using ProductApi.Models;
using SharedModels;
using System;
using System.Threading;

namespace ProductApi.Infrastructure
{
    public class MessageListener
    {
        IServiceProvider provider;
        string connectionString;

        // The service provider is passed as a parameter, because the class needs
        // access to the product repository. With the service provider, we can create
        // a service scope that can provide an instance of the product repository.
        public MessageListener(IServiceProvider provider, string connectionString)
        {
            this.provider = provider;
            this.connectionString = connectionString;
        }

        public void Start()
        {
            using (var bus = RabbitHutch.CreateBus(connectionString))
            {
                bus.PubSub.Subscribe<OrderStatusChangedMessage>("productApiCompleteID",
                                                                HandleOrderCompleted,
                                                                x => x.WithTopic("completed"));

                bus.PubSub.Subscribe<OrderStatusChangedMessage>("productApiCancelID",
                                                                HandleOrderCanceled,
                                                                x => x.WithTopic("cancelled"));

                bus.PubSub.Subscribe<OrderStatusChangedMessage>("productApiSendID",
                                                                HandleOrderSent,
                                                                x => x.WithTopic("sent"));

                // Block the thread so that it will not exit and stop subscribing.
                lock (this)
                {
                    Monitor.Wait(this);
                }
            }
        }

        private void HandleOrderCompleted(OrderStatusChangedMessage message)
        {
            // A service scope is created to get an instance of the product repository.
            // When the service scope is disposed, the product repository instance will
            // also be disposed.
            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var productRepos = services.GetService<IRepository<Product>>();

                // Reserve items of ordered product (should be a single transaction).
                // Beware that this operation is not idempotent.
                foreach (var orderLine in message.OrderLines)
                {
                    var product = productRepos.Get(orderLine.ProductId);
                    product.ItemsReserved += orderLine.Quantity;
                    productRepos.Edit(product);
                }
            }
        }

        private void HandleOrderCanceled(OrderStatusChangedMessage message)
        {
            // A service scope is created to get an instance of the product repository.
            // When the service scope is disposed, the product repository instance will
            // also be disposed.
            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var productRepos = services.GetService<IRepository<Product>>();

                // Reserve items of ordered product (should be a single transaction).
                // Beware that this operation is not idempotent.
                foreach (OrderLine orderLine in message.OrderLines)
                {
                    Product product = productRepos.Get(orderLine.ProductId);
                    product.ItemsReserved -= orderLine.Quantity;
                    productRepos.Edit(product);
                }
            }
        }

        private void HandleOrderSent(OrderStatusChangedMessage message)
        {
            // A service scope is created to get an instance of the product repository.
            // When the service scope is disposed, the product repository instance will
            // also be disposed.
            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var productRepos = services.GetService<IRepository<Product>>();

                // Reserve items of ordered product (should be a single transaction).
                // Beware that this operation is not idempotent.
                foreach (OrderLine orderLine in message.OrderLines)
                {
                    Product product = productRepos.Get(orderLine.ProductId);
                    product.ItemsReserved -= orderLine.Quantity;
                    product.ItemsInStock -= orderLine.Quantity;
                    productRepos.Edit(product);
                }
            }
        }

    }
}

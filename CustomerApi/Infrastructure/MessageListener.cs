using CustomerApi.Data;
using CustomerApi.Models;
using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;
using SharedModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerApi.Infrastructure
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
                bus.PubSub.Subscribe<OrderStatusChangedMessage>("customerApiCompletedID",
                                                                HandleOrderCompleted,
                                                                x => x.WithTopic("completed"));

                bus.PubSub.Subscribe<OrderStatusChangedMessage>("productApiCancelID",
                                                                HandleOrderPaidOrCancelled,
                                                                x => x.WithTopic("cancelled"));

                bus.PubSub.Subscribe<OrderStatusChangedMessage>("productApiPayID",
                                                                HandleOrderPaidOrCancelled,
                                                                x => x.WithTopic("paid"));

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
                var customersRepo = services.GetService<IRepository<Customer>>();

                Customer customer = customersRepo.Get(message.CustomerId);
                customer.CreditStanding += message.Amount;

                customersRepo.Edit(customer);
            }
        }

        private void HandleOrderPaidOrCancelled(OrderStatusChangedMessage message)
        {
            // A service scope is created to get an instance of the product repository.
            // When the service scope is disposed, the product repository instance will
            // also be disposed.
            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var customersRepo = services.GetService<IRepository<Customer>>();

                Customer customer = customersRepo.Get(message.CustomerId);
                customer.CreditStanding -= message.Amount;

                customersRepo.Edit(customer);
            }
        }

    }
}

using EasyNetQ;
using EmailApi.Data;
using EmailApi.Models;
using Microsoft.Extensions.DependencyInjection;
using SharedModels;
using System;
using System.Linq;
using System.Threading;

namespace EmailApi.Infrastructure
{
    public class MessageListener
    {
        IServiceProvider provider;
        string connectionString;

        public MessageListener(IServiceProvider provider, string connectionString)
        {
            this.provider = provider;
            this.connectionString = connectionString;
        }

        public void Start()
        {
            using (var bus = RabbitHutch.CreateBus(connectionString))
            {
                bus.PubSub.Subscribe<SendEmailMessage>("emailApiSendEmailID",
                                                       HandleSendEmail,
                                                       x => x.WithTopic("sendEmail"));

                // Block the thread so that it will not exit and stop subscribing.
                lock (this)
                {
                    Monitor.Wait(this);
                }
            }
        }

        private void HandleSendEmail(SendEmailMessage message)
        {
            // A service scope is created to get an instance of the product repository.
            // When the service scope is disposed, the product repository instance will
            // also be disposed.
            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var emailRepos = services.GetService<IRepository<Email>>();

                int nextId = emailRepos.GetAll().Count() + 1;
                Email email = new Email()
                {
                    Id = nextId,
                    Destination = message.Destination,
                    Content = message.Content
                };

                emailRepos.Add(email);
            }
        }

    }
}

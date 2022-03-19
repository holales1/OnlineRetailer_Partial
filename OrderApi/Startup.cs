using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderApi.Data;
using OrderApi.Infrastructure;
using SharedModels;

namespace OrderApi
{
    public class Startup
    {
        string productServiceUrl = "http://productapi/products/";
        string customerServiceUrl = "http://customerapi/customers/";
        string emailServiceUrl = "http://emailapi/emails/";

        string cloudAMQPConnectionString =
           "host=roedeer.rmq.cloudamqp.com;virtualHost=mlmsucqa;username=mlmsucqa;password=ie6BkUxeRm2WhugOZerChu99Fn4rC635";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // In-memory database:
            services.AddDbContext<OrderApiContext>(opt => opt.UseInMemoryDatabase("OrdersDb"));

            // Register repositories for dependency injection
            services.AddScoped<IRepository<Order>, OrderRepository>();

            services.AddScoped<IRepositoryOrderLine<OrderLine>, OrderLineRepository>();

            // Register database initializer for dependency injection
            services.AddTransient<IDbInitializer, DbInitializer>();

            // Register product service gateway for dependency injection
            services.AddSingleton<IServiceGateway<ProductDto>>(new ProductServiceGateway(productServiceUrl));

            // Register customer service gateway for dependency injection
            services.AddSingleton<IServiceGateway<CustomerDto>>(new CustomerServiceGateway(customerServiceUrl));

            // Register email service gateway for dependency injection
            services.AddSingleton<IServiceGateway<EmailDto>>(new EmailServiceGateway(emailServiceUrl));

            // Register MessagePublisher (a messaging gateway) for dependency injection
            services.AddSingleton<IMessagePublisher>(new MessagePublisher(cloudAMQPConnectionString));

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Initialize the database
            using (var scope = app.ApplicationServices.CreateScope())
            {
                // Initialize the database
                var services = scope.ServiceProvider;
                var dbContext = services.GetService<OrderApiContext>();
                var dbInitializer = services.GetService<IDbInitializer>();
                dbInitializer.Initialize(dbContext);
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

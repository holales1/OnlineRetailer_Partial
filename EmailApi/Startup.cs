using EmailApi.Data;
using EmailApi.Infrastructure;
using EmailApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedModels;
using System.Threading.Tasks;

namespace EmailApi
{
    public class Startup
    {
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
            services.AddDbContext<EmailApiContext>(opt => opt.UseInMemoryDatabase("EmailsDb"));

            // Register repositories for dependency injection
            services.AddScoped<IRepository<Email>, EmailRepository>();

            // Register database initializer for dependency injection
            services.AddTransient<IDbInitializer, DbInitializer>();

            // Register ProductConverter for dependency injection
            services.AddSingleton<IConverter<Email, EmailDto>, EmailConverter>();

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
                var dbContext = services.GetService<EmailApiContext>();
                var dbInitializer = services.GetService<IDbInitializer>();
                dbInitializer.Initialize(dbContext);
            }

            // Create a message listener in a separate thread.
            Task.Factory.StartNew(() =>
                new MessageListener(app.ApplicationServices, cloudAMQPConnectionString).Start());

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

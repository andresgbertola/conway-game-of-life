using GOL.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GOL.WebApi.Tests
{
    public class GOLWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove all EF Core related services
                var descriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<GOLDbContext>) ||
                               d.ServiceType == typeof(GOLDbContext))
                    .ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                // Create a singleton service provider for the in-memory database
                var inMemoryProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                // Add DbContext using an in-memory database for testing
                services.AddDbContext<GOLDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting")
                          .UseInternalServiceProvider(inMemoryProvider);
                });

                // Initialize the database
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<GOLDbContext>();
                    db.Database.EnsureCreated();
                }
            });
        }
    }
}

using GOL.Domain.Interfaces;
using GOL.Domain.Repositories;
using GOL.Domain.Services;
using GOL.Infrastructure.Data;
using GOL.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;

namespace ProcessBoardStateWorkerService
{
    public static class DI
    {
        public static void AddDomain(this IServiceCollection services)
        {
            services.AddScoped<IBoardStateRepository, BoardStateRepository>();
            services.AddSingleton<IGameOfLifeService, GameOfLifeService>();
            services.AddTransient<IBoardStateProcessingService, BoardStateProcessingService>();
        }

        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<GOLDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("GameOfLifeDb")));

            services.AddAzureClients(builder =>
            {
                builder.AddServiceBusClient(configuration.GetConnectionString("BoardStateProcessingQueue"));
            });
        }
    }
}

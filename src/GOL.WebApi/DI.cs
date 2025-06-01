using GOL.Application.Commands;
using GOL.Application.Mapper;
using GOL.Application.Queries;
using GOL.Application.Validators;
using GOL.Domain.Interfaces;
using GOL.Domain.Repositories;
using GOL.Domain.Services;
using GOL.Infrastructure.Data;
using GOL.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace GOL.WebApi
{
    public static class DI
    {
        public static void AddDomain(this IServiceCollection services)
        {
            services.AddScoped<IBoardStateRepository, BoardStateRepository>();
            services.AddSingleton<IGameOfLifeService, GameOfLifeService>();
        }

        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<GOLDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("GameOfLifeDb")));
        }

        public static void AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg => 
                cfg.RegisterServicesFromAssembly(typeof(GetLastBoardStateByIdQuery).Assembly)
            );
            services.AddAutoMapper(typeof(BoardStateMappingProfile));
            services.AddSingleton<IValidator<CreateNewBoardCommand>, CreateNewBoardCommandValidator>();
            services.AddSingleton<IValidator<UpdateBoardStatusCommand>, UpdateBoardStatusValidator>();
        }
    }

    public partial class Program { }
}

using GOL.Infrastructure.Data;
using GOL.WebApi;
using GOL.WebApi.Middlewares;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // XML file from the Web.Api project.
    var webApiXml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    options.IncludeXmlComments(webApiXml);
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(configuration);
builder.Services.AddDomain();

var app = builder.Build();

// Automatically apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<GOLDbContext>();
    if (dbContext.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
    {
        dbContext.Database.Migrate();
    }
}


// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
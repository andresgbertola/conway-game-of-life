using ProcessBoardStateWorkerService;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var configuration = builder.Configuration;

builder.Services.AddDomain();
builder.Services.AddInfrastructure(configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

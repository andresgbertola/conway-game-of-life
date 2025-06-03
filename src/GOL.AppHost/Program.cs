using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var serviceBus = builder.AddAzureServiceBus("LocalAzureServiceBus")
                        .RunAsEmulator();

var queue = serviceBus.AddServiceBusQueue("BoardStateProcessingQueue");

// Add a SQL Server container resource
var dbPassword = builder.AddParameter("DbPassword", secret: true);

var sql = builder.AddSqlServer("sql", dbPassword)
                 .WithLifetime(ContainerLifetime.Persistent)
                 .WithDataVolume();

var db = sql.AddDatabase("GameOfLifeDb");

// Connect the Web API project to the database
builder.AddProject<Projects.GOL_WebApi>("gol-webapi")
    .WithReference(db)
    .WithReference(queue)
    .WaitFor(db)
    .WaitFor(queue);

builder.AddProject<Projects.ProcessBoardStateWorkerService>("processboardstateworkerservice")    
    .WithReference(db)
    .WithReference(queue)
    .WaitFor(db)
    .WaitFor(queue);

builder.Build().Run();
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);


// Add a SQL Server container resource
var dbPassword = builder.AddParameter("DbPassword", secret: true);

var sql = builder.AddSqlServer("sql", dbPassword)
                 .WithLifetime(ContainerLifetime.Persistent)
                 .WithDataVolume();

var db = sql.AddDatabase("GameOfLifeDb");

// Connect the Web API project to the database
builder.AddProject<Projects.GOL_WebApi>("gol-webapi")
    .WithReference(db)
    .WaitFor(db);

builder.Build().Run();
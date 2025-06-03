using Azure.Messaging.ServiceBus;
using GOL.Domain.Interfaces;
using GOL.Messaging;
using System.Text.Json;

namespace ProcessBoardStateWorkerService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ServiceBusProcessor _processor;
    private const string QueueName = "BoardStateProcessingQueue";

    public Worker(ILogger<Worker> logger,
                  IServiceProvider serviceProvider,
                  ServiceBusClient serviceBusClient)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        var processorOptions = new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = true,
        };
        _processor = serviceBusClient.CreateProcessor(QueueName, processorOptions);
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _processor.ProcessMessageAsync += ProcessMessageHandler;
        _processor.ProcessErrorAsync += ProcessErrorHandler;
        await _processor.StartProcessingAsync(cancellationToken);
        _logger.LogInformation("Started processing messages from queue: {QueueName}", QueueName);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Keep the service running until a cancellation is requested.
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _processor.StopProcessingAsync(cancellationToken);
        await _processor.DisposeAsync();
        _logger.LogInformation("Stopped processing messages from queue: {QueueName}", QueueName);
        await base.StopAsync(cancellationToken);
    }

    private async Task ProcessMessageHandler(ProcessMessageEventArgs args)
    {
        _logger.LogInformation("Received message");

        using (var scope = _serviceProvider.CreateScope())
        {
            var boardStateProcessingService = scope.ServiceProvider.GetRequiredService<IBoardStateProcessingService>();

            try
            {
                var scheduleMessage = await JsonSerializer.DeserializeAsync<ScheduleBoardStateExecutionMessage>(args.Message.Body.ToStream());

                await boardStateProcessingService.ProcessBoardIterationsUntilEndAsync(scheduleMessage!.BoardId, cancellationToken: args.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing board state.");
            }
        }
    }

    private Task ProcessErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Error while processing message from queue: {ErrorSource}", args.ErrorSource);
        return Task.CompletedTask;
    }
}

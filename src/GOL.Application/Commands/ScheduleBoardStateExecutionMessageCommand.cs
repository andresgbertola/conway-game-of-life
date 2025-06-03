using Azure.Messaging.ServiceBus;
using GOL.Domain.Exceptions;
using GOL.Domain.Repositories;
using GOL.Messaging;
using MediatR;
using System.Text.Json;

namespace GOL.Application.Commands
{
    /// <summary>
    /// Command to send a board state message using the board identifier.
    /// </summary>
    public record ScheduleBoardStateExecutionMessageCommand(Guid BoardId) : IRequest;

    /// <summary>
    /// Handler for SendBoardStateMessageCommand. Triggers the domain service to send a ServiceBus message.
    /// </summary>
    public class ScheduleBoardStateExecutionMessageCommandHandler : IRequestHandler<ScheduleBoardStateExecutionMessageCommand>
    {
        private readonly ServiceBusSender _boardQueueSender;
        private readonly IBoardStateRepository _boardStateRepository;

        public ScheduleBoardStateExecutionMessageCommandHandler(ServiceBusClient serviceBusClient, IBoardStateRepository boardStateRepository)
        {
            _boardQueueSender = serviceBusClient.CreateSender("BoardStateProcessingQueue");
            _boardStateRepository = boardStateRepository;
        }

        public async Task Handle(ScheduleBoardStateExecutionMessageCommand request, CancellationToken cancellationToken)
        {
            _ = await _boardStateRepository.GetLatestByBoardIdAsync(request.BoardId) ?? throw new NotFoundException($"Board with ID {request.BoardId} does not exist.");

            var messageBody = JsonSerializer.Serialize(new ScheduleBoardStateExecutionMessage(request.BoardId));
            var message = new ServiceBusMessage(messageBody);

            await _boardQueueSender.SendMessageAsync(message);
        }
    }
}


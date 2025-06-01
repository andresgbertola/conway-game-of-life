using AutoMapper;
using GOL.Application.DTOs;
using GOL.Application.Exceptions;
using GOL.Application.Validators;
using GOL.Domain.Entities;
using GOL.Domain.Enums;
using GOL.Domain.Interfaces;
using GOL.Domain.Repositories;
using MediatR;

namespace GOL.Application.Commands
{
    /// <summary>
    /// Command to Update Board Status.
    /// </summary>
    public sealed record UpdateBoardStatusCommand(Guid BoardId, long Iterations, bool ShortCircuitFinalState) : IRequest<BoardStateDto>;

    /// <summary>
    /// Handler.
    /// </summary>
    public class UpdateBoardStatusCommandHandler : IRequestHandler<UpdateBoardStatusCommand, BoardStateDto>
    {
        private readonly IBoardStateRepository _boardStateRepository;
        private readonly IGameOfLifeService _gameOfLifeService;
        private readonly IMapper _mapper;
        private readonly IValidator<UpdateBoardStatusCommand> _validator;

        public UpdateBoardStatusCommandHandler(IBoardStateRepository boardStateRepository,
            IGameOfLifeService gameOfLifeService,
            IMapper mapper,
            IValidator<UpdateBoardStatusCommand> validator)
        {
            _boardStateRepository = boardStateRepository;
            _gameOfLifeService = gameOfLifeService;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<BoardStateDto> Handle(UpdateBoardStatusCommand request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var validationResult = _validator.Validate(request);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            // Retrieve the latest BoardState for the given board.
            var currentState = await _boardStateRepository.GetLatestByBoardIdAsync(request.BoardId);
            
            if (currentState == null) throw new NotFoundException($"Board Id={request.BoardId} not found");

            // Process up to maxAttempts new generations.
            for (int i = 1; i <= request.Iterations; i++)
            {
                // Compute the next generation.
                var newState = _gameOfLifeService.ComputeNextGeneration(currentState.LiveCells);

                // Create a new BoardState record.
                var newBoardState = new BoardState(newState, request.BoardId, currentState);

                // If not finished: 
                if (currentState.Status == State.NotFinished)
                {
                    // Check if the board has faded away.
                    if (newBoardState.LiveCells.Count == 0)
                    {
                        newBoardState.Status = State.FadedAway;
                    }
                    else
                    {
                        // Check for a previous state with the same hash.
                        var previousState = await _boardStateRepository.GetLastestByBoardIdAndHashAsync(request.BoardId, newBoardState.StateHash);

                        // The same hash already exists, so the state is finished.
                        var hasEnded = previousState != null && previousState.Id != newBoardState.Id;
                        if (hasEnded)
                        {
                            long period = newBoardState.Iteration - previousState!.Iteration;
                            newBoardState.Status = (period == 1) ? State.Stable : State.Oscillatory;
                        }
                    }                    
                }

                // Persist the new state.
                await _boardStateRepository.AddAsync(newBoardState);
                await _boardStateRepository.SaveChangesAsync();

                // If it finished and it is waiting on the final state.
                if (newBoardState.Status != State.NotFinished && request.ShortCircuitFinalState)
                    return _mapper.Map<BoardStateDto>(newBoardState);

                // If not returning early, continue with the new state.
                currentState = newBoardState;
            }

            // If it was asked that it should run until finished and did not reach to conclusion, return error:
            // 422 Unprocessable Entity is being used because it signals that while the request was syntactically valid and processing occurred,
            // the resulting state does not meet the required business criteria.
            if (request.ShortCircuitFinalState && currentState.Status == State.NotFinished)
                throw new CustomErrorException($"After {request.Iterations} iterations, the board did not go to conclusion.", 422);

            return _mapper.Map<BoardStateDto>(currentState);
        }
    }
}

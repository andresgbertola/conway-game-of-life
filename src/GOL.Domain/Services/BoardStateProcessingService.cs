using GOL.Domain.Entities;
using GOL.Domain.Enums;
using GOL.Domain.Exceptions;
using GOL.Domain.Interfaces;
using GOL.Domain.Repositories;

namespace GOL.Domain.Services
{
    public class BoardStateProcessingService : IBoardStateProcessingService
    {
        private readonly IBoardStateRepository _boardStateRepository;
        private readonly IGameOfLifeService _gameOfLifeService;

        const int MaxAttemptsToDeclareItInfinite = 10000;

        public BoardStateProcessingService(
            IBoardStateRepository boardStateRepository,
            IGameOfLifeService gameOfLifeService)
        {
            _boardStateRepository = boardStateRepository;
            _gameOfLifeService = gameOfLifeService;
        }

        public async Task ProcessBoardIterationsUntilEndAsync(Guid boardId, CancellationToken cancellationToken)
        {
            await ProcessBoardIterationsAsync(boardId, MaxAttemptsToDeclareItInfinite, true, cancellationToken);
        }

        public async Task<BoardState> ProcessBoardIterationsAsync(
            Guid boardId,
            long iterations,
            bool shortCircuitFinalState,
            CancellationToken cancellationToken)
        {
            var currentState = await _boardStateRepository.GetLatestByBoardIdAsync(boardId);
            
            if (currentState == null) 
                throw new NotFoundException($"Board Id={boardId} not found");

            // Process up to maxAttempts new generations
            for (int i = 1; i <= iterations; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Compute the next generation
                var newState = _gameOfLifeService.ComputeNextGeneration(currentState.LiveCells);

                // Create a new BoardState record
                var newBoardState = new BoardState(newState, boardId, currentState);

                // If not finished: 
                if (currentState.Status == State.NotFinished)
                {
                    // Check if the board has faded away
                    if (newBoardState.LiveCells.Count == 0)
                    {
                        newBoardState.Status = State.FadedAway;
                    }
                    else
                    {
                        // Check for a previous state with the same hash
                        var previousState = await _boardStateRepository.GetLastestByBoardIdAndHashAsync(boardId, newBoardState.StateHash);

                        // The same hash already exists, so the state is finished
                        var hasEnded = previousState != null && previousState.Id != newBoardState.Id;
                        if (hasEnded)
                        {
                            long period = newBoardState.Iteration - previousState!.Iteration;
                            newBoardState.Status = (period == 1) ? State.Stable : State.Oscillatory;
                        }
                        else if (newBoardState.Iteration == MaxAttemptsToDeclareItInfinite)
                        {
                            newBoardState.Status = State.Infinite;
                        }
                    }                    
                }

                // Persist the new state
                await _boardStateRepository.AddAsync(newBoardState);
                await _boardStateRepository.SaveChangesAsync();

                // If it finished and it is waiting on the final state
                if (newBoardState.Status != State.NotFinished && shortCircuitFinalState)
                    return newBoardState;

                // If not returning early, continue with the new state
                currentState = newBoardState;
            }

            // If it was asked that it should run until finished and did not reach to conclusion
            if (shortCircuitFinalState && currentState.Status == State.NotFinished)
                throw new CustomErrorException($"After {iterations} iterations, the board did not go to conclusion.", 422);

            return currentState;
        }
    }
}
using GOL.Domain.Entities;
using GOL.Domain.Enums;
using GOL.Domain.Exceptions;
using GOL.Domain.Interfaces;
using GOL.Domain.Repositories;
using GOL.Domain.Services;
using Moq;

namespace GOL.Domain.Tests.Services
{
    public class BoardStateProcessingServiceTests
    {
        private readonly Mock<IBoardStateRepository> _repoMock;
        private readonly Mock<IGameOfLifeService> _gameServiceMock;
        private readonly BoardStateProcessingService _service;

        public BoardStateProcessingServiceTests()
        {
            _repoMock = new Mock<IBoardStateRepository>();
            _gameServiceMock = new Mock<IGameOfLifeService>();
            _service = new BoardStateProcessingService(_repoMock.Object, _gameServiceMock.Object);
        }

        [Fact]
        public async Task ProcessBoardIterationsAsync_BoardNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var boardId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetLatestByBoardIdAsync(boardId))
                .ReturnsAsync((BoardState)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                _service.ProcessBoardIterationsAsync(boardId, 1, false, CancellationToken.None));
        }

        [Fact]
        public async Task ProcessBoardIterationsAsync_EmptyBoard_ShortCircuitsAndReturnsFadedAway()
        {
            // Arrange
            var boardId = Guid.NewGuid();
            var emptyBoardState = new BoardState(new List<CellCoordinates>(), boardId);
            emptyBoardState.Status = State.NotFinished;

            _repoMock.Setup(r => r.GetLatestByBoardIdAsync(boardId))
                .ReturnsAsync(emptyBoardState);

            _gameServiceMock.Setup(g => g.ComputeNextGeneration(It.IsAny<List<CellCoordinates>>()))
                .Returns(new List<CellCoordinates>());

            // Act
            var result = await _service.ProcessBoardIterationsAsync(boardId, 5, true, CancellationToken.None);

            // Assert
            Assert.Equal(State.FadedAway, result.Status);
            Assert.Empty(result.LiveCells);
            Assert.Equal(boardId, result.BoardId);

            _gameServiceMock.Verify(r => r.ComputeNextGeneration(It.IsAny<List<CellCoordinates>>()), Times.Once);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<BoardState>()), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ProcessBoardIterationsAsync_EmptyBoard_ContinuesAllIterations()
        {
            // Arrange
            var boardId = Guid.NewGuid();
            var emptyBoardState = new BoardState(new List<CellCoordinates>(), boardId);
            emptyBoardState.Status = State.NotFinished;

            _repoMock.Setup(r => r.GetLatestByBoardIdAsync(boardId))
                .ReturnsAsync(emptyBoardState);

            _gameServiceMock.Setup(g => g.ComputeNextGeneration(It.IsAny<List<CellCoordinates>>()))
                .Returns(new List<CellCoordinates>());

            // Act
            var result = await _service.ProcessBoardIterationsAsync(boardId, 5, false, CancellationToken.None);

            // Assert
            Assert.Equal(State.FadedAway, result.Status);
            Assert.Empty(result.LiveCells);

            _gameServiceMock.Verify(r => r.ComputeNextGeneration(It.IsAny<List<CellCoordinates>>()), Times.Exactly(5));
            _repoMock.Verify(r => r.AddAsync(It.IsAny<BoardState>()), Times.Exactly(5));
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Exactly(5));
        }

        [Fact]
        public async Task ProcessBoardIterationsAsync_StablePattern_DetectsAndReturnsStable()
        {
            // Arrange
            var boardId = Guid.NewGuid();
            var liveCells = new List<CellCoordinates> { new(0, 1), new(1, 0) };
            var currentState = new BoardState(liveCells, boardId);
            currentState.Status = State.NotFinished;

            _repoMock.Setup(r => r.GetLatestByBoardIdAsync(boardId))
                .ReturnsAsync(currentState);

            _gameServiceMock.Setup(g => g.ComputeNextGeneration(It.IsAny<List<CellCoordinates>>()))
                .Returns(liveCells);

            _repoMock.Setup(r => r.GetLastestByBoardIdAndHashAsync(boardId, It.IsAny<ulong>()))
                .ReturnsAsync(currentState);

            // Act
            var result = await _service.ProcessBoardIterationsAsync(boardId, 5, true, CancellationToken.None);

            // Assert
            Assert.Equal(State.Stable, result.Status);
            Assert.Equal(2, result.LiveCells.Count);
        }

        [Fact]
        public async Task ProcessBoardIterationsAsync_OscillatoryPattern_DetectsAndReturnsOscillatory()
        {
            // Arrange
            var boardId = Guid.NewGuid();
            var initialState = new BoardState(new List<CellCoordinates> { new(0, 0), new(0, 1) }, boardId);
            
            // Create a previous state with iteration 1
            var intermediateState = new BoardState(initialState.LiveCells, boardId, initialState);
            
            // Create the state that matches the pattern but at iteration 3
            var previousState = new BoardState(initialState.LiveCells, boardId, intermediateState);
            var previousStateWithMoreIterations = new BoardState(previousState.LiveCells, boardId, previousState);

            _repoMock.Setup(r => r.GetLatestByBoardIdAsync(boardId))
                .ReturnsAsync(initialState);

            _gameServiceMock.Setup(g => g.ComputeNextGeneration(It.IsAny<List<CellCoordinates>>()))
                .Returns(initialState.LiveCells);

            _repoMock.Setup(r => r.GetLastestByBoardIdAndHashAsync(boardId, It.IsAny<ulong>()))
                .ReturnsAsync(previousStateWithMoreIterations);

            // Act
            var result = await _service.ProcessBoardIterationsAsync(boardId, 5, true, CancellationToken.None);

            // Assert
            Assert.Equal(State.Oscillatory, result.Status);
        }

        [Fact]
        public async Task ProcessBoardIterationsAsync_NotFinishedWithShortCircuit_ThrowsCustomErrorException()
        {
            // Arrange
            var boardId = Guid.NewGuid();
            var initialLiveCells = new List<CellCoordinates> { new(0, 0), new(0, 1) };
            var initialState = new BoardState(initialLiveCells, boardId);

            _repoMock.Setup(r => r.GetLatestByBoardIdAsync(boardId))
                .ReturnsAsync(initialState);

            _gameServiceMock.Setup(g => g.ComputeNextGeneration(It.IsAny<List<CellCoordinates>>()))
                .Returns(new List<CellCoordinates> { new(1, 1) }); // Different pattern

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomErrorException>(() => 
                _service.ProcessBoardIterationsAsync(boardId, 3, true, CancellationToken.None));

            Assert.Equal(422, exception.HttpStatusCode);
            Assert.Contains("did not go to conclusion", exception.Message);
        }

        [Fact]
        public async Task ProcessBoardIterationsAsync_CancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            var boardId = Guid.NewGuid();
            var currentState = new BoardState(new List<CellCoordinates> { new(0, 0) }, boardId);
            var cts = new CancellationTokenSource();

            _repoMock.Setup(r => r.GetLatestByBoardIdAsync(boardId))
                .ReturnsAsync(currentState);

            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => 
                _service.ProcessBoardIterationsAsync(boardId, 1, false, cts.Token));
        }
    }
}
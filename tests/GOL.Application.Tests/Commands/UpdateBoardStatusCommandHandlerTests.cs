using Moq;
using GOL.Application.Commands;
using GOL.Application.DTOs;
using GOL.Application.Exceptions;
using GOL.Application.Validators;
using GOL.Domain.Entities;
using GOL.Domain.Enums;
using GOL.Domain.Interfaces;
using GOL.Domain.Repositories;
using AutoMapper;
using GOL.Application.Mapper;

namespace GOL.Application.Tests
{
    public class UpdateBoardStatusCommandHandlerTests
    {
        private readonly Mock<IBoardStateRepository> _repoMock;
        private readonly Mock<IGameOfLifeService> _gameServiceMock;
        private readonly IMapper _mapper;
        private readonly Mock<IValidator<UpdateBoardStatusCommand>> _validatorMock;
        private readonly UpdateBoardStatusCommandHandler _handler;

        public UpdateBoardStatusCommandHandlerTests()
        {
            _repoMock = new Mock<IBoardStateRepository>();
            _gameServiceMock = new Mock<IGameOfLifeService>();

            // Configure AutoMapper with the real mapping profile.
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<BoardStateMappingProfile>();
            });

            _mapper = config.CreateMapper();
            _validatorMock = new Mock<IValidator<UpdateBoardStatusCommand>>();

            // By default, set validator to succeed.
            _validatorMock.Setup(v => v.Validate(It.IsAny<UpdateBoardStatusCommand>()))
                          .Returns(new GOL.Application.Validators.ValidationResult());

            _handler = new UpdateBoardStatusCommandHandler(
                _repoMock.Object,
                _gameServiceMock.Object,
                _mapper,
                _validatorMock.Object);
        }

        [Fact]
        public async Task Handle_NullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ValidationFails_ThrowsValidationException()
        {
            // Arrange
            var dto = new CreateBoardRequestDto { LiveCells = new List<CellCoordinates> { new(0, 1), new(1, 0) } };
            var command = new UpdateBoardStatusCommand(Guid.NewGuid(), 1, false);

            // Setup validator to fail.
            var failedResult = new ValidationResult();
            failedResult.Errors.Add("Validation error");
            _validatorMock.Setup(v => v.Validate(It.IsAny<UpdateBoardStatusCommand>()))
                          .Returns(failedResult);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_BoardNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var boardId = Guid.NewGuid();
            var command = new UpdateBoardStatusCommand(boardId, 1, false);

            // Setup repository to return null for GetLatestByBoardIdAsync.
            _repoMock.Setup(r => r.GetLatestByBoardIdAsync(boardId))
                     .ReturnsAsync((BoardState)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_EmptyBoard_ShortCircuitsAndReturnsFadedAwayDto()
        {
            // Arrange: Create a BoardState that is empty.
            var boardId = Guid.NewGuid();
            var emptyLiveCells = new List<CellCoordinates> { };

            // Create a board state with an empty board.
            var emptyBoardState = new BoardState(emptyLiveCells, boardId);
            emptyBoardState.Status = State.NotFinished; // initially not finished

            // Setup repository to return this board state.
            _repoMock.Setup(r => r.GetLatestByBoardIdAsync(boardId))
                     .ReturnsAsync(emptyBoardState);

            _gameServiceMock.Setup(g => g.ComputeNextGeneration(It.IsAny<List<CellCoordinates>>()))
                            .Returns(emptyLiveCells);

            var command = new UpdateBoardStatusCommand(boardId, 5, true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert: The status should be updated to FadedAway.
            Assert.Equal(State.FadedAway.ToString(), result.Status);
            Assert.Equal(boardId, result.BoardId);

            _gameServiceMock.Verify(r => r.ComputeNextGeneration(It.IsAny<List<CellCoordinates>>()), Times.Once,
                "GameService.ComputeNextGeneration should be called at least once.");            
            _repoMock.Verify(r => r.AddAsync(It.IsAny<BoardState>()), Times.Once,
                "Repository.AddAsync should be called at least once.");
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once,
                "Repository.SaveChangesAsync should be called at least once.");
        }

        [Fact]
        public async Task Handle_EmptyBoard_ShortCircuitsFalseAndReturnsFadedAwayDto()
        {
            // Arrange: Create a BoardState that is empty.
            var boardId = Guid.NewGuid();
            var emptyLiveCells = new List<CellCoordinates> { };

            // Create a board state with an empty board.
            var emptyBoardState = new BoardState(emptyLiveCells, boardId);
            emptyBoardState.Status = State.NotFinished; // initially not finished

            var emptyBoardStateFadedAway = new BoardState(emptyLiveCells, boardId);
            emptyBoardStateFadedAway.Status = State.FadedAway;

            // Setup repository to return this board state.
            // Returns NotFinished first, but then as it finished but shortCircuit is in false it continues
            // but with FadedAway status.
            var callCount = 0;
            _repoMock.Setup(r => r.GetLatestByBoardIdAsync(boardId))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    return callCount == 1 ? emptyBoardState : emptyBoardStateFadedAway;
                });

            _gameServiceMock.Setup(g => g.ComputeNextGeneration(It.IsAny<List<CellCoordinates>>()))
                            .Returns(emptyLiveCells);

            var command = new UpdateBoardStatusCommand(boardId, 5, false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert: The status should be updated to FadedAway.
            Assert.Equal(State.FadedAway.ToString(), result.Status);
            Assert.Equal(boardId, result.BoardId);

            _gameServiceMock.Verify(r => r.ComputeNextGeneration(It.IsAny<List<CellCoordinates>>()), Times.Exactly(5),
                "GameService.ComputeNextGeneration should be called 5 times.");
            _repoMock.Verify(r => r.AddAsync(It.IsAny<BoardState>()), Times.Exactly(5),
                "Repository.AddAsync should be called 5 times.");
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Exactly(5),
                "Repository.SaveChangesAsync should be called 5 times.");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsMappedBoardStateDto()
        {
            // Arrange: Create a valid board state.
            var boardId = Guid.NewGuid();
            var initialLivingCells = new List<CellCoordinates>
            {
                new(0, 1), new(1, 0)
            };

            var currentState = new BoardState(initialLivingCells, boardId);
            currentState.Status = State.NotFinished;

            // Setup repository to return current state.
            _repoMock.Setup(r => r.GetLatestByBoardIdAsync(boardId))
                     .ReturnsAsync(currentState);

            // Setup the game service to simulate one generation update.
            // For simplicity, assume the next generation is exactly the same as the initial.
            _gameServiceMock.Setup(g => g.ComputeNextGeneration(It.IsAny<List<CellCoordinates>>()))
                            .Returns(initialLivingCells);

            // Setup repository for adding new board state.
            _repoMock.Setup(r => r.AddAsync(It.IsAny<BoardState>()))
                     .Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChangesAsync())
                     .Returns(Task.CompletedTask);

            // Create a command with a single iteration and no short-circuit.
            var command = new UpdateBoardStatusCommand(boardId, 1, false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert:
            // - Ensure the repository's AddAsync and SaveChangesAsync were called once.
            _repoMock.Verify(r => r.AddAsync(It.IsAny<BoardState>()), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            // - Verify the mapped result.
            Assert.Equal(boardId, result.BoardId);
            Assert.Equal(1, result.Iteration);
            Assert.Equal(State.NotFinished.ToString(), result.Status);
        }

        [Fact]
        public async Task Handle_ShouldThrowCustomErrorException_WhenFinalStateNotReached()
        {
            // Arrange: Prepare test inputs.
            var boardId = Guid.NewGuid();
            // Command: Ask for 3 iterations and request early exit if a final state is not reached.
            var command = new UpdateBoardStatusCommand(boardId, 3, true);

            // Set up the validator to return a valid result (no errors).
            var validationResult = new ValidationResult(); // Assuming a valid result when Errors is empty.
            _validatorMock
                .Setup(v => v.Validate(It.IsAny<UpdateBoardStatusCommand>()))
                .Returns(validationResult);

            // Create an initial board state that is "NotFinished".
            // For simplicity, we simulate live cells as a list of coordinate tuples.
            var initialLiveCells = new List<CellCoordinates>
            {
                new(0, 0),
                new(0, 1)
            };
            // Create an initial BoardState (with no previous state).
            var initialBoardState = new BoardState(initialLiveCells, boardId);

            // Set up repository to return the initial board state when requested.
            _repoMock
                .Setup(r => r.GetLatestByBoardIdAsync(boardId))
                .ReturnsAsync(initialBoardState);

            // Ensure that checking for a previous state always returns null, so the board never "finishes".
            _repoMock
                .Setup(r => r.GetLastestByBoardIdAndHashAsync(boardId, It.IsAny<ulong>()))
                .ReturnsAsync((BoardState)null);

            // Set up the Game of Life service to always return the same set of live cells,
            // so the board remains NotFinished in every generation.
            _gameServiceMock
                .Setup(s => s.ComputeNextGeneration(It.IsAny<List<CellCoordinates>>()))
                .Returns(initialLiveCells);

            // Set up repository methods for persisting the new state. These can simply complete.
            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<BoardState>()))
                .Returns(Task.CompletedTask);
            _repoMock
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act & Assert: Expect the handler to throw a CustomErrorException,
            // since after 3 iterations, the board state is still NotFinished.
            var exception = await Assert.ThrowsAsync<CustomErrorException>(
                () => _handler.Handle(command, CancellationToken.None));

            // Verify that the exception has the expected status code (422) and message content.
            Assert.Equal(422, exception.HttpStatusCode);
            Assert.Contains("did not go to conclusion", exception.Message);
        }
    }
}

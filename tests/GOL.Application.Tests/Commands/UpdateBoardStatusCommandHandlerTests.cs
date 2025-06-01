using Moq;
using GOL.Application.Commands;
using GOL.Domain.Exceptions;
using GOL.Application.Validators;
using GOL.Domain.Entities;
using GOL.Domain.Enums;
using GOL.Domain.Interfaces;
using AutoMapper;
using GOL.Application.Mapper;

namespace GOL.Application.Tests
{
    public class UpdateBoardStatusCommandHandlerTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IValidator<UpdateBoardStatusCommand>> _validatorMock;
        private readonly Mock<IBoardStateProcessingService> _boardStateProcessingServiceMock;
        private readonly UpdateBoardStatusCommandHandler _handler;

        public UpdateBoardStatusCommandHandlerTests()
        {
            _boardStateProcessingServiceMock = new Mock<IBoardStateProcessingService>();

            // Configure AutoMapper with the real mapping profile
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<BoardStateMappingProfile>();
            });

            _mapper = config.CreateMapper();
            _validatorMock = new Mock<IValidator<UpdateBoardStatusCommand>>();

            // By default, set validator to succeed
            _validatorMock.Setup(v => v.Validate(It.IsAny<UpdateBoardStatusCommand>()))
                          .Returns(new ValidationResult());

            _handler = new UpdateBoardStatusCommandHandler(
                _boardStateProcessingServiceMock.Object,
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
            var command = new UpdateBoardStatusCommand(Guid.NewGuid(), 1, false);

            // Setup validator to fail
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

            _boardStateProcessingServiceMock
                .Setup(s => s.ProcessBoardIterationsAsync(
                    boardId, 1, false, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException($"Board Id={boardId} not found"));

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ProcessingFailure_ThrowsCustomErrorException()
        {
            // Arrange
            var boardId = Guid.NewGuid();
            var command = new UpdateBoardStatusCommand(boardId, 3, true);

            _boardStateProcessingServiceMock
                .Setup(s => s.ProcessBoardIterationsAsync(
                    boardId, 3, true, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new CustomErrorException("After 3 iterations, the board did not go to conclusion.", 422));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomErrorException>(() => 
                _handler.Handle(command, CancellationToken.None));

            Assert.Equal(422, exception.HttpStatusCode);
            Assert.Contains("did not go to conclusion", exception.Message);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsMappedBoardStateDto()
        {
            // Arrange
            var boardId = Guid.NewGuid();
            var command = new UpdateBoardStatusCommand(boardId, 1, false);

            var expectedBoardState = new BoardState(
                new List<CellCoordinates> { new(0, 1), new(1, 0) },
                boardId);
            expectedBoardState.Status = State.NotFinished;

            _boardStateProcessingServiceMock
                .Setup(s => s.ProcessBoardIterationsAsync(
                    boardId, 1, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedBoardState);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(boardId, result.BoardId);
            Assert.Equal(State.NotFinished.ToString(), result.Status);
            Assert.Equal(2, result.LiveCells.Count);
            
            _boardStateProcessingServiceMock.Verify(
                s => s.ProcessBoardIterationsAsync(
                    boardId, 1, false, It.IsAny<CancellationToken>()), 
                Times.Once);
        }
    }
}

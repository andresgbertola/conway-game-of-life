using Moq;
using GOL.Application.Commands;
using GOL.Application.DTOs;
using GOL.Application.Exceptions;
using GOL.Application.Validators;
using GOL.Domain.Entities;
using GOL.Domain.Repositories;

namespace GOL.Application.Tests
{
    public class CreateNewBoardCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ThrowsArgumentNullException_WhenRequestIsNull()
        {
            // Arrange
            var repositoryMock = new Mock<IBoardStateRepository>();
            var validatorMock = new Mock<IValidator<CreateNewBoardCommand>>();
            var handler = new CreateNewBoardCommandHandler(repositoryMock.Object, validatorMock.Object);

            // Act & Assert: Passing a null request should throw ArgumentNullException.
            await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(null, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ThrowsValidationException_WhenValidatorFails()
        {
            // Arrange: Create a valid DTO.
            var dto = new CreateBoardRequestDto
            {
                LiveCells = new List<CellCoordinates>
                {
                    new(0, 1), new(1, 0)
                }
            };
            var command = new CreateNewBoardCommand(dto);

            var repositoryMock = new Mock<IBoardStateRepository>();

            // Setup validator to return an invalid result.
            var validationResult = new GOL.Application.Validators.ValidationResult();
            validationResult.Errors.Add("Invalid board state");
            var validatorMock = new Mock<IValidator<CreateNewBoardCommand>>();
            validatorMock.Setup(v => v.Validate(It.IsAny<CreateNewBoardCommand>()))
                         .Returns(validationResult);

            var handler = new CreateNewBoardCommandHandler(repositoryMock.Object, validatorMock.Object);

            // Act & Assert: The handler should throw a ValidationException.
            await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ReturnsBoardId_WhenValid()
        {
            // Arrange: Create a valid DTO.
            var dto = new CreateBoardRequestDto
            {
                LiveCells = new List<CellCoordinates>
                {
                    new(0, 1), new(1, 0)
                }
            };

            var command = new CreateNewBoardCommand(dto);

            // Setup repository mock.
            var repositoryMock = new Mock<IBoardStateRepository>();
            repositoryMock.Setup(r => r.AddAsync(It.IsAny<BoardState>()))
                          .Returns(Task.CompletedTask);
            repositoryMock.Setup(r => r.SaveChangesAsync())
                          .Returns(Task.CompletedTask);

            // Setup validator mock to return a valid result.
            var validationResult = new GOL.Application.Validators.ValidationResult(); // no errors, IsValid==true
            var validatorMock = new Mock<IValidator<CreateNewBoardCommand>>();
            validatorMock.Setup(v => v.Validate(It.IsAny<CreateNewBoardCommand>()))
                         .Returns(validationResult);

            var handler = new CreateNewBoardCommandHandler(repositoryMock.Object, validatorMock.Object);

            // Act: Call the handler.
            Guid returnedBoardId = await handler.Handle(command, CancellationToken.None);

            // Assert:
            // Verify that repository.AddAsync was called with a BoardState whose BoardId equals returnedBoardId.
            repositoryMock.Verify(r => r.AddAsync(It.Is<BoardState>(b => b.BoardId == returnedBoardId)), Times.Once);
            repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            Assert.NotEqual(Guid.Empty, returnedBoardId);
        }
    }
}

using GOL.Application.Commands;
using GOL.Application.DTOs;
using GOL.Domain.Exceptions;
using GOL.Domain.Entities;

namespace GOL.Application.Tests
{
    public class CreateNewBoardCommandValidatorTests
    {
        private readonly CreateNewBoardCommandValidator _validator;

        public CreateNewBoardCommandValidatorTests()
        {
            _validator = new CreateNewBoardCommandValidator();
        }

        [Fact]
        public void Validate_NullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert: Passing a null request should throw ArgumentNullException.
            Assert.Throws<ArgumentNullException>(() => _validator.Validate(null));
        }

        [Fact]
        public void Validate_NullStateInDto_ReturnsError()
        {
            // Arrange: Create a DTO with State = null.
            var dto = new CreateBoardRequestDto { LiveCells = null };
            // The constructor won't throw here since it checks for null in its own way.
            var command = new CreateNewBoardCommand(dto);

            // Act
            var result = _validator.Validate(command);

            // Assert: Expect error "State cannot be null."
            Assert.False(result.IsValid);
            Assert.Contains("LiveCells cannot be null.", result.Errors);
        }

        [Fact]
        public void Validate_Empty_ReturnsError()
        {
            // Arrange: Create a DTO with an empty board.
            var dto = new CreateBoardRequestDto { LiveCells = new List<CellCoordinates> { } };
            var command = new CreateNewBoardCommand(dto);

            // Act
            var result = _validator.Validate(command);

            // Assert: Expect error "State must have at least one row."
            Assert.False(result.IsValid);
            Assert.Contains("LiveCells must have at least one row.", result.Errors);
        }

        [Fact]
        public void Validate_ValidBoard_ReturnsValidResult()
        {
            // Arrange: Create a valid board.
            var board = new List<CellCoordinates>
            {
                new(0, 1), new(1, 0)
            };

            var dto = new CreateBoardRequestDto { LiveCells = board };
            var command = new CreateNewBoardCommand(dto);

            // Act
            var result = _validator.Validate(command);

            // Assert: Expect no errors.
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }
    }
}

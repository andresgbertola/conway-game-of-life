using Microsoft.Extensions.Configuration;
using GOL.Application.Commands;

namespace GOL.Application.Tests.Commands
{
    public class UpdateBoardStatusValidatorTests
    {
        private readonly IConfiguration _configuration;
        private readonly int _maxIterations = 1000;

        public UpdateBoardStatusValidatorTests()
        {
            // Create in-memory configuration for testing.
            var inMemorySettings = new Dictionary<string, string>
            {
                { "BoardConfig:MaxIterations", _maxIterations.ToString() }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [Fact]
        public void Validate_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var validator = new UpdateBoardStatusValidator(_configuration);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => validator.Validate(null));
        }

        [Fact]
        public void Validate_EmptyBoardId_ReturnsError()
        {
            // Arrange: Create a command with an empty BoardId.
            var validator = new UpdateBoardStatusValidator(_configuration);
            var command = new UpdateBoardStatusCommand(Guid.Empty, 10, false);

            // Act
            var result = validator.Validate(command);

            // Assert: Expect an error indicating the BoardId was not set.
            Assert.False(result.IsValid);
            Assert.Contains("BoardId was not set.", result.Errors);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Validate_IterationsBelowMinimum_ReturnsError(int iterations)
        {
            // Arrange: iterations below the minimum (must be at least 1)
            var validator = new UpdateBoardStatusValidator(_configuration);
            var command = new UpdateBoardStatusCommand(Guid.NewGuid(), iterations, false);

            // Act
            var result = validator.Validate(command);

            // Assert: The error message should indicate the valid range.
            Assert.False(result.IsValid);
            Assert.Contains($"Iteration value: {iterations} should be between 1 and {_maxIterations}.", result.Errors);
        }

        [Fact]
        public void Validate_IterationsAboveMaximum_ReturnsError()
        {
            // Arrange: iterations above the maximum.
            int iterations = _maxIterations + 1;
            var validator = new UpdateBoardStatusValidator(_configuration);
            var command = new UpdateBoardStatusCommand(Guid.NewGuid(), iterations, false);

            // Act
            var result = validator.Validate(command);

            // Assert: Expect an error message.
            Assert.False(result.IsValid);
            Assert.Contains($"Iteration value: {iterations} should be between 1 and {_maxIterations}.", result.Errors);
        }

        [Fact]
        public void Validate_ValidCommand_ReturnsValidResult()
        {
            // Arrange: Create a valid command.
            int iterations = 500;
            var boardId = Guid.NewGuid();
            var validator = new UpdateBoardStatusValidator(_configuration);
            var command = new UpdateBoardStatusCommand(boardId, iterations, false);

            // Act
            var result = validator.Validate(command);

            // Assert: There should be no errors.
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }
    }
}

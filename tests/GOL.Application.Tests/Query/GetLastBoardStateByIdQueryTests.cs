using AutoMapper;
using FluentAssertions;
using Moq;
using GOL.Application.DTOs;
using GOL.Application.Exceptions;
using GOL.Application.Queries;
using GOL.Domain.Entities;
using GOL.Domain.Repositories;

namespace GOL.Application.Tests.Query
{
    public class GetLastBoardStateByIdQueryHandlerTests
    {
        private readonly Mock<IBoardStateRepository> _repositoryMock;
        private readonly IMapper _mapper;
        private readonly GetLastBoardStateByIdQueryHandler _handler;

        public GetLastBoardStateByIdQueryHandlerTests()
        {
            _repositoryMock = new Mock<IBoardStateRepository>();

            // Create a real AutoMapper instance with a minimal mapping profile.
            var config = new MapperConfiguration(cfg =>
            {
                // Map BoardState to BoardStateDto.
                cfg.CreateMap<BoardState, BoardStateDto>();
            });
            _mapper = config.CreateMapper();

            _handler = new GetLastBoardStateByIdQueryHandler(_repositoryMock.Object, _mapper);
        }

        [Fact]
        public async Task Handle_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            GetLastBoardStateByIdQuery request = null;

            // Act & Assert: Passing a null request should throw an ArgumentNullException.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(request, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_EmptyId_ThrowsValidationException()
        {
            // Arrange: Create a query with an empty Guid.
            var request = new GetLastBoardStateByIdQuery(Guid.Empty);

            // Act & Assert: Should throw a ValidationException because the Id was not set.
            await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(request, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_BoardNotFound_ThrowsNotFoundException()
        {
            // Arrange: Create a query with a valid Guid.
            var boardId = Guid.NewGuid();
            var request = new GetLastBoardStateByIdQuery(boardId);

            // Setup repository to return null.
            _repositoryMock.Setup(r => r.GetLatestByBoardIdAsync(boardId))
                           .ReturnsAsync((BoardState)null);

            // Act & Assert: Expect a NotFoundException.
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(request, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsMappedBoardStateDto()
        {
            // Arrange: Create a query with a valid boardId.
            var boardId = Guid.NewGuid();
            var request = new GetLastBoardStateByIdQuery(boardId);

            // Create a sample board state.
            var matrix = new List<CellCoordinates>
            {
                new (0,0), new (1,1)
            };
            var boardState = new BoardState(matrix, boardId);

            // Setup repository to return the board state.
            _repositoryMock.Setup(r => r.GetLatestByBoardIdAsync(boardId))
                           .ReturnsAsync(boardState);

            // Act: Handle the request.
            BoardStateDto result = await _handler.Handle(request, CancellationToken.None);

            // Assert: Validate that the mapped DTO matches the board state.
            result.Should().NotBeNull();
            result.BoardId.Should().Be(boardId);
            result.Iteration.Should().Be(boardState.Iteration);
            // Additional assertions for properties can be added as needed.
        }
    }
}

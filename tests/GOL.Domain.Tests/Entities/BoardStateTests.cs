using FluentAssertions;
using GOL.Domain.Entities;
using GOL.Domain.Enums;
using GOL.Tests.Shared;

namespace GOL.Domain.Tests.Entities
{
    public class BoardStateTests
    {
        [Fact]
        public void Constructor_ValidInput_InitializesPropertiesCorrectly()
        {
            // Arrange:
            var board = new List<CellCoordinates>
            {
                new (0,1),
                new (1,0)
            };

            // Hash is done based on State value.
            ulong expectedBoardHash = 11125509564030907931;
            string expectedStateJson = "[[0,1],[1,0]]";

            // Act: Create a new BoardState.
            var boardState = new BoardState(board);

            // Assert:
            // - Id and BoardId should be non-empty.
            Assert.NotEqual(Guid.Empty, boardState.Id);
            Assert.NotEqual(Guid.Empty, boardState.BoardId);
            // - Initial iteration is 0 and default status is NotFinished.
            Assert.Equal(0, boardState.Iteration);
            Assert.Equal(State.NotFinished, boardState.Status);

            // - The State property equals the JSON-serialized board.
            Assert.Equal(expectedStateJson, boardState.State);

            // - The computed hash matches the expected hash.
            Assert.Equal(expectedBoardHash, boardState.StateHash);

            // - The LiveCells property returns a board equal to the input.
            board.Should().BeEquivalentTo(boardState.LiveCells);
        }

        [Fact]
        public void Constructor_WithPreviousBoard_IncrementsIterationAndCopiesStatus()
        {
            // Arrange: Create an initial board and a previous BoardState.
            
            var board1 = new List<CellCoordinates>
            {
                new(0, 1), new(1, 0)
            };

            var previous = new BoardState(board1);
            
            // Create a new board.
            var board2 = new List<CellCoordinates>
            {
                new(0, 0), new(0, 1)
            };

            // Hash is done based on State value.
            ulong expectedBoardHash = 11126444148914698056;
            string expectedStateJson = "[[0,0],[0,1]]";

            // Act: Create a new BoardState with previous board provided.
            var boardState = new BoardState(board2, boardStateId: previous.BoardId, previousBoard: previous);

            // Assert:
            // - Iteration is previous.Iteration + 1.
            Assert.Equal(previous.Iteration + 1, boardState.Iteration);
            // - BoardId is preserved.
            Assert.Equal(previous.BoardId, boardState.BoardId);
            // - Status is copied from previous.
            Assert.Equal(previous.Status, boardState.Status);
            // - The State property equals the JSON-serialized board.
            Assert.Equal(expectedStateJson, boardState.State);
            // - The computed hash matches the expected hash.
            Assert.Equal(expectedBoardHash, boardState.StateHash);
        }        
    }
}

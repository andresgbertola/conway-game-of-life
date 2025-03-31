using FluentAssertions;
using GOL.Domain.Entities;
using GOL.Domain.Services; 
using GOL.Tests.Shared; 

namespace GOL.Domain.Tests.Services
{
    public class GameOfLifeServiceTests
    {
        private readonly GameOfLifeService _gameOfLifeService;

        public GameOfLifeServiceTests()
        {
            _gameOfLifeService = new GameOfLifeService();
        }

        #region Single Live Cell

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        public void ComputeNextGeneration_SingleLiveCell_Dies(int row, int col)
        {
            var cell = new CellCoordinates(row, col);

            // Arrange: For a 2x2 board, a single live cell dies.
            var input = new List<CellCoordinates> { cell };

            // Act
            var result = _gameOfLifeService.ComputeNextGeneration(input);

            // Assert: Expect an empty list.
            result.Should().BeEmpty("A single cell dies from underpopulation.");
        }

        #endregion

        #region Oscillators and Stable Patterns

        [Fact]
        public void ComputeNextGeneration_BlinkerOscillator_TogglesState()
        {
            // Arrange: Vertical blinker (in a virtual 5x5 context)
            // Coordinates: (1,2), (2,2), (3,2)
            var input = new List<CellCoordinates> { new (1, 2), new (2, 2), new (3, 2) };

            // Act: Next generation of a vertical blinker should be a horizontal line.
            var result = _gameOfLifeService.ComputeNextGeneration(input);

            // Expected: Horizontal blinker at row 2: (2,1), (2,2), (2,3)
            var expected = new List<CellCoordinates> { new (2, 1), new (2, 2), new (2, 3) };

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ComputeNextGeneration_BlockStable_RemainsUnchanged()
        {
            // Arrange: Stable 2x2 block
            // Coordinates: (1,1), (1,2), (2,1), (2,2)
            var input = new List<CellCoordinates> { new(1, 1), new(1, 2), new(2, 1), new(2, 2) };

            // Act
            var result = _gameOfLifeService.ComputeNextGeneration(input);

            // Expected: The same block remains.
            var expected = new List<CellCoordinates> { new(1, 1), new(1, 2), new(2, 1), new(2, 2) };

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ComputeNextGeneration_BeehiveStable_RemainsUnchanged()
        {
            // Arrange: Beehive pattern (from a 6x6 board).
            // Live cells: (1,2), (1,3), (2,1), (2,4), (3,2), (3,3)
            var input = new List<CellCoordinates>
            {
                new (1,2), new(1, 3), new(2, 1), new(2, 4), new(3, 2), new(3, 3)
            };

            // Act
            var result = _gameOfLifeService.ComputeNextGeneration(input);

            // Expected: A beehive is stable, so the same coordinates remain.
            var expected = new List<CellCoordinates>
            {
                new (1,2), new (1,3), new (2,1), new (2,4),new  (3,2), new (3,3)
            };

            result.Should().BeEquivalentTo(expected);
        }

        #endregion

        #region Glider

        [Fact]
        public void ComputeNextGeneration_Glider_MovesDiagonally()
        {
            // Arrange: Canonical glider pattern.
            // Standard glider (offset by (2,2)): (2,3), (3,4), (4,2), (4,3), (4,4)
            var input = new List<CellCoordinates>
            {
                new (2,3), new (3,4), new (4,2), new (4,3), new (4,4)
            };

            // Act: Compute next generation.
            var result = _gameOfLifeService.ComputeNextGeneration(input);

            // Canonical glider moves down-right one cell.
            // One common outcome: live cells at (3,2), (3,4), (4,3), (4,4), (5,3).
            var expected = new List<CellCoordinates>
            {
                new(3, 2), new(3, 4), new(4, 3), new(4, 4), new(5, 3)
            };

            result.Should().BeEquivalentTo(expected);
        }

        #endregion

        #region Complex / Multiple Iterations

        [Fact]
        public void ComputeNextGeneration_Cross_Expansion()
        {
            // Arrange: Cross pattern from a 7x3 matrix.
            // Row0:        (0,1)
            // Row1:        (1,1)
            // Row2: (2,0), (2,1), (2,2)
            // Row3:        (3,1)
            // Row4:        (4,1)
            // Row5:        (5,1)
            // Row6:        (6,1)
            var input = new List<CellCoordinates>
            {
                            new(0, 1),
                            new(1, 1),
                new(2, 0),  new(2, 1), new(2, 2),
                            new(3, 1),
                            new(4, 1),
                            new(5, 1),
                            new(6, 1)
            };

            // Assume we have a helper that loads the expected final state for the cross pattern
            // as a sparse representation (List<CellCoordinates>).
            var expected = Helpers.GetStateFromJsonFile("CrossStateExpected");

            // Act: Simulate 177 iterations.
            var actual = input;
            for (int i = 1; i <= 177; i++)
            {
                actual = _gameOfLifeService.ComputeNextGeneration(actual);
            }

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ComputeNextGeneration_FullyLiveBoard_CornersSurvive()
        {
            // Arrange: Create a full 3x3 board where every cell is live.
            var input = new List<CellCoordinates>();
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    input.Add(new CellCoordinates(row, col));
                }
            }

            var expected = new List<CellCoordinates>
            {
                new (-1, 1 ),
                new (1, -1),
                new (0,  0 ),
                new (0,  2 ),
                new (1,  3 ),
                new (2,  0 ),
                new (2,  2 ),
                new (3,  1 )
            };

            // Act: Compute the next generation from the sparse representation.
            var result = _gameOfLifeService.ComputeNextGeneration(input);

            var value = Serializers.CellCoordinatesSerializer.Serialize(result);

            // Assert: The resulting list of live cell coordinates should match the expected corners.
            result.Should().BeEquivalentTo(expected);
        }

        #endregion
    }
}

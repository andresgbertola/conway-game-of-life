using System.Text;
using System.Text.Json;
using FluentAssertions;
using GOL.Application.DTOs;
using GOL.Domain.Entities;
using GOL.Domain.Enums;
using GOL.Tests.Shared;

namespace GOL.WebApi.Tests.BoardStateController
{
    public class BoardStateControllerTests : IClassFixture<GOLWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public BoardStateControllerTests(GOLWebApplicationFactory<Program> factory)
        {
            // Create an HttpClient instance from the custom factory.
            _client = factory.CreateClient();
        }

        /// <summary>
        /// Integration Tests for BoardStateController
        //1.	Create a new board and verify its initial state:
        //    •	Create a new board with a specific initial state.
        //    •	Retrieve the board state and verify it matches the expected initial state.
        //2.	Retrieve the next state of the board:
        //    •	Request the next state of the board and verify it matches the expected next state.
        //3.	Retrieve the state of the board after a specified number of iterations:
        //    •	Request the state of the board after 10 iterations and verify it matches the expected state.
        //4.	Attempt to reach the final state with a limited number of iterations:
        //    •	Attempt to reach the final state of the board with a maximum of 100 iterations and verify it fails with an UnprocessableEntity status code.
        //    •	Verify that the board's state has progressed by the expected number of iterations.
        //5.	Retrieve the final state of the board:
        //    •	Request the final state of the board with a maximum of 1000 iterations and verify it matches the expected final state.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateBoardAndGetBoardState_ReturnsOkResponse()
        {
            // Arrange: Create a sample board.
            var newBoardRequest = new CreateBoardRequestDto
            {
                LiveCells = new List<CellCoordinates>
                {
                            new (0,1),
                            new (1,1),
                    new (2,0),new (2,1), new (2,2),
                            new (3,1),
                            new (4,1),
                            new (5,1),
                            new (6,1)
                }
            };

            var expectedInitialResponse = new BoardStateDto
            {
                // The BoardId is unknown until after creation.
                LiveCells = newBoardRequest.LiveCells,
                Iteration = 0,
                Status = "NotFinished"
            };

            // Act: Create new board.
            Guid boardId = await CreateBoardAsync(newBoardRequest);
            boardId.Should().NotBe(Guid.Empty);

            // Act: Retrieve board state.
            var boardState = await GetBoardStateAsync(boardId);
            expectedInitialResponse.BoardId = boardId;

            // Assert: Initial state is mapped as expected.
            boardState.Should().BeEquivalentTo(expectedInitialResponse);

            // Arrange: Expected next state.
            var expectedNextResponse = new BoardStateDto
            {
                BoardId = boardId,
                LiveCells = new List<CellCoordinates> 
                {
                        new(2, 0), new(2, 2),
                        new(4, 0), new(4, 1), new(4, 2),
                        new(5, 0), new(5, 1), new(5, 2)
                },
                Iteration = 1,
                Status = expectedInitialResponse.Status
            };

            // Act: Get next state.
            var nextBoardState = await GetNextStateAsync(boardId);

            // Assert: Next state is as expected.
            nextBoardState.Should().BeEquivalentTo(expectedNextResponse);

            // Arrange: Expected next 10 steps state.
            var nextIterations = 10;
            var expectedNextStepsResponse = new BoardStateDto
            {
                BoardId = boardId,
                LiveCells = new List<CellCoordinates>
                {
                    new (3, -1),
                    new (4, -1),
                    new (5, -3),
                    new (5, -2),
                    new (4, -2),
                    new (6, -2),
                    new (6, -1),
                    new (7, -1), new (7, 0),
                    new (8, 0),
                    new (7, 1),
                    new (8, 1),
                    new (9, 1),
                    new (7, 2),
                    new (8, 2),
                    new (7, 3),
                    new (6, 3),
                    new (6, 4),
                    new (4, 3),
                    new (4, 4),
                    new (5, 5),
                    new (5, 4),
                    new (3, 3)
                },
                Iteration = nextIterations + nextBoardState.Iteration,
                Status = expectedInitialResponse.Status
            };

            // Act: Get next 10 steps state.
            var nextStepsBoardState = await GetNextStepsStateAsync(boardId, nextIterations);

            // Assert: Next 10 steps state is as expected.
            nextStepsBoardState.Should().BeEquivalentTo(expectedNextStepsResponse);

            // Arrange: Expected final state. Not reached and gets error
            var firstFinalIterations = 100;

            // Act: Try to get final state with 100 steps max attemps.
            var response = await _client.PostAsync($"/api/BoardState/{boardId}/final?maxAttempts={firstFinalIterations}", null);
            var content = await response.Content.ReadAsStringAsync();

            // Assert: Reaching final value failed.
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity, $"Response code should be {System.Net.HttpStatusCode.UnprocessableEntity}");

            // Act: Retrieve current board state.
            var boardStateAfterReachingEndFailed = await GetBoardStateAsync(boardId);

            // Assert: Initial state is mapped as expected.
            Assert.True(boardStateAfterReachingEndFailed.Iteration == nextStepsBoardState.Iteration + firstFinalIterations);

            // Arrange: Expected final state.
            var lastFinalIterations = 1000;

            var expectedFinalStateResponse = new BoardStateDto
            {
                BoardId = boardId,
                LiveCells = Helpers.GetStateFromJsonFile("CrossStateExpected"),
                Iteration = 177,
                Status = State.Oscillatory.ToString()
            };

            // Act: Get next 10 steps state.
            var finalBoardState = await GetFinalStateAsync(boardId, lastFinalIterations);

            // Assert: Final state is as expected.
            finalBoardState.Should().BeEquivalentTo(expectedFinalStateResponse);
        }

        // Helper method: Create a new board and return its Guid.
        private async Task<Guid> CreateBoardAsync(CreateBoardRequestDto newBoardRequest)
        {
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(newBoardRequest),
                Encoding.UTF8,
                "application/json");

            var response = await _client.PostAsync("/api/BoardState", jsonContent);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Guid>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        // Helper method: Retrieve the board state for a given board id.
        private async Task<BoardStateDto> GetBoardStateAsync(Guid boardId)
        {
            var response = await _client.GetAsync($"/api/BoardState/{boardId}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<BoardStateDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        // Helper method: Retrieve the next state of the board.
        private async Task<BoardStateDto> GetNextStateAsync(Guid boardId)
        {
            var response = await _client.PostAsync($"/api/BoardState/{boardId}/next", null);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<BoardStateDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private async Task<BoardStateDto> GetNextStepsStateAsync(Guid boardId, int steps)
        {
            var response = await _client.PostAsync($"/api/BoardState/{boardId}/next/{steps}", null);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<BoardStateDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private async Task<BoardStateDto> GetFinalStateAsync(Guid boardId, int maxAttempts)
        {
            var response = await _client.PostAsync($"/api/BoardState/{boardId}/final?maxAttempts={maxAttempts}", null);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<BoardStateDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}

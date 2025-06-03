using GOL.Application.Commands;
using GOL.Application.DTOs;
using GOL.Application.Queries;
using GOL.WebApi.Middlewares;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GOL.WebApi.Controllers
{
    /// <summary>
    /// Board State controller.
    /// </summary>
    [Route("api/[controller]")]
    public class BoardStateController : Controller
    {
        private readonly IMediator _mediator;

        public BoardStateController(IMediator mediator)
        {
            this._mediator = mediator;
        }


        /// <summary>
        /// Gets the current board state of the boardId.
        /// </summary>
        /// <param name="boardId">Board identifier</param>
        /// <returns></returns>
        [HttpGet("{boardId}")]
        [ProducesResponseType(typeof(BoardStateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetBoardState(Guid boardId)
        {
            var response = await _mediator.Send(new GetLastBoardStateByIdQuery(boardId));

            return Ok(response);
        }

        /// <summary>
        /// Creates a new board with the defined initial live cells.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateBoard([FromBody]CreateBoardRequestDto request)
        {
            var response = await _mediator.Send(new CreateNewBoardCommand(request));

            return Ok(response);
        }

        /// <summary>
        /// Advances the board one step and returns the new state.
        /// </summary>
        /// <param name="boardId">Board identifier</param>
        /// <returns></returns>
        [HttpPost("{boardId}/next")]
        [ProducesResponseType(typeof(BoardStateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetNextState(Guid boardId)
        {
            var response = await _mediator.Send(new UpdateBoardStatusCommand(boardId, 1, false));

            return Ok(response);
        }

        /// <summary>
        /// Advances the board the number of defined steps and returns the new state.
        /// </summary>
        /// <param name="boardId"></param>
        /// <param name="steps"></param>
        /// <returns></returns>
        [HttpPost("{boardId}/next/{steps:int}")]
        [ProducesResponseType(typeof(BoardStateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetStateAfterSteps(Guid boardId, long steps)
        {
            var response = await _mediator.Send(new UpdateBoardStatusCommand(boardId, steps, false));

            return Ok(response);
        }

        /// <summary>
        /// Advances the board until a final state is reached (stable, oscillatory, or faded away) and returns the state.
        /// If the final state is not reached within the maxAttempts, an error is returned.
        /// </summary>
        [HttpPost("{boardId}/final")]
        [ProducesResponseType(typeof(BoardStateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> GetFinalState(Guid boardId, [FromQuery] long maxAttempts = 2000)
        {
            var response = await _mediator.Send(new UpdateBoardStatusCommand(boardId, maxAttempts, true));

            return Ok(response);
        }

        /// <summary>
        /// Schedules the board state for asynchronous execution.
        /// </summary>
        /// <param name="boardId"></param>
        /// <param name="maxAttempts"></param>
        /// <returns></returns>
        [HttpPost("{boardId}/scheduleAsyncExecution")]
        [ProducesResponseType(typeof(BoardStateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ScheduleAsyncExecution(Guid boardId)
        {
            await _mediator.Send(new ScheduleBoardStateExecutionMessageCommand(boardId));

            return Accepted();
        }
    }
}

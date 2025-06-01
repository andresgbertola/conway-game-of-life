using AutoMapper;
using GOL.Application.DTOs;
using GOL.Application.Exceptions;
using GOL.Domain.Repositories;
using MediatR;

namespace GOL.Application.Queries
{
    /// <summary>
    /// Query to get the last board state.
    /// </summary>
    public sealed record GetLastBoardStateByIdQuery(Guid Id) : IRequest<BoardStateDto>;

    /// <summary>
    /// Handler.
    /// </summary>
    public class GetLastBoardStateByIdQueryHandler : IRequestHandler<GetLastBoardStateByIdQuery, BoardStateDto>
    {
        private readonly IBoardStateRepository _boardRepository;
        private readonly IMapper _mapper;

        public GetLastBoardStateByIdQueryHandler(IBoardStateRepository boardRepository, IMapper mapper)
        {
            _boardRepository = boardRepository;
            _mapper = mapper;
        }

        public async Task<BoardStateDto> Handle(GetLastBoardStateByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));

            if (request.Id == Guid.Empty) throw new ValidationException($"{nameof(request.Id)} was not set.");

            var board = await _boardRepository.GetLatestByBoardIdAsync(request.Id);

            if (board == null) throw new NotFoundException($"Board Id={request.Id} not found");

            return _mapper.Map<BoardStateDto>(board);
        }
    }
}

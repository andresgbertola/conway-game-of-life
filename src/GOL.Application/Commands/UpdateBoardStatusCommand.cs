using AutoMapper;
using GOL.Application.DTOs;
using GOL.Domain.Exceptions;
using GOL.Application.Validators;
using GOL.Domain.Interfaces;
using MediatR;

namespace GOL.Application.Commands
{
    public sealed record UpdateBoardStatusCommand(Guid BoardId, long Iterations, bool ShortCircuitFinalState) : IRequest<BoardStateDto>;

    public class UpdateBoardStatusCommandHandler : IRequestHandler<UpdateBoardStatusCommand, BoardStateDto>
    {
        private readonly IBoardStateProcessingService _boardStateProcessingService;
        private readonly IMapper _mapper;
        private readonly IValidator<UpdateBoardStatusCommand> _validator;

        public UpdateBoardStatusCommandHandler(
            IBoardStateProcessingService boardStateProcessingService,
            IMapper mapper,
            IValidator<UpdateBoardStatusCommand> validator)
        {
            _boardStateProcessingService = boardStateProcessingService;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<BoardStateDto> Handle(UpdateBoardStatusCommand request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var validationResult = _validator.Validate(request);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

                var result = await _boardStateProcessingService.ProcessBoardIterationsAsync(
                    request.BoardId,
                    request.Iterations,
                    request.ShortCircuitFinalState,
                    cancellationToken);

            return _mapper.Map<BoardStateDto>(result);
        }
    }
}

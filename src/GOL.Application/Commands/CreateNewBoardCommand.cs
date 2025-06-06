﻿using GOL.Application.DTOs;
using GOL.Application.Validators;
using GOL.Domain.Entities;
using GOL.Domain.Exceptions;
using GOL.Domain.Repositories;
using MediatR;

namespace GOL.Application.Commands
{
    /// <summary>
    /// Command to create a new board.
    /// </summary>
    public sealed record CreateNewBoardCommand(CreateBoardRequestDto CreateBoardDto) : IRequest<Guid>;

    /// <summary>
    /// Handler class.
    /// </summary>
    public class CreateNewBoardCommandHandler : IRequestHandler<CreateNewBoardCommand, Guid>
    {
        private readonly IBoardStateRepository _boardStateRepository;
        private readonly IValidator<CreateNewBoardCommand> _validator;

        public CreateNewBoardCommandHandler(IBoardStateRepository boardStateRepository, IValidator<CreateNewBoardCommand> boardValidator)
        {
            _boardStateRepository = boardStateRepository;
            _validator = boardValidator;
        }

        public async Task<Guid> Handle(CreateNewBoardCommand request, CancellationToken cancellationToken)
        {
            if (request is null) throw new ValidationException("Empty body.");

            var validationResult = _validator.Validate(request);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var boardState = new BoardState(request.CreateBoardDto.LiveCells);
            await _boardStateRepository.AddAsync(boardState);
            await _boardStateRepository.SaveChangesAsync();

            return boardState.BoardId;
        }
    }
}

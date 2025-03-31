using GOL.Application.Validators;

namespace GOL.Application.Commands
{
    /// <summary>
    /// Create New Board Command Validator.
    /// </summary>
    public class CreateNewBoardCommandValidator : IValidator<CreateNewBoardCommand>
    {
        public ValidationResult Validate(CreateNewBoardCommand request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var newBoard = request.CreateBoardDto;
            var result = new ValidationResult();

            if (newBoard is null)
            {
                result.Errors.Add("The request cannot be null.");
                return result;
            }

            var board = newBoard.LiveCells;

            // Validate board is not null.
            if (board == null)
            {
                result.Errors.Add($"{nameof(newBoard.LiveCells)} cannot be null.");
                return result;
            }

            // Validate board has at least one row.
            if (board.Count == 0)
            {
                result.Errors.Add($"{nameof(newBoard.LiveCells)} must have at least one row.");
            }            

            return result;
        }
    }
}

using GOL.Application.Validators;
using Microsoft.Extensions.Configuration;

namespace GOL.Application.Commands
{
    /// <summary>
    /// Update Board Status Command Validator.
    /// </summary>
    public class UpdateBoardStatusValidator : IValidator<UpdateBoardStatusCommand>
    {
        private readonly int _maxIterations;

        public UpdateBoardStatusValidator(IConfiguration configuration)
        {
            _maxIterations = configuration.GetValue<int>("BoardConfig:MaxIterations");

        }

        public ValidationResult Validate(UpdateBoardStatusCommand request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = new ValidationResult();

            // Validates BoardId
            if (request.BoardId == Guid.Empty)
            {
                result.Errors.Add($"{nameof(request.BoardId)} was not set.");
                return result;
            }

            // Validates Iterations
            if (request.Iterations < 1 || request.Iterations > _maxIterations)
            {
                result.Errors.Add($"Iteration value: {request.Iterations} should be between 1 and {_maxIterations}.");
                return result;
            }

            return result;
        }
    }
}

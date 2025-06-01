using GOL.Domain.Exceptions;

namespace GOL.WebApi.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation error: {Errors}", string.Join(", ", ex.Errors));

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                var response = new ApiError
                {
                    Message = "Validation failed.",
                    Errors = ex.Errors,
                    Timestamp = DateTime.UtcNow
                };
                await context.Response.WriteAsJsonAsync(response);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("Not found error: {Message}", ex.Message);

                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "application/json";
                var response = new ApiError
                {
                    Message = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
                await context.Response.WriteAsJsonAsync(response);
            }
            catch (CustomErrorException ex)
            {
                _logger.LogWarning("Not found error: {Message}", ex.Message);

                context.Response.StatusCode = ex.HttpStatusCode;
                context.Response.ContentType = "application/json";
                var response = new ApiError
                {
                    Message = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
                await context.Response.WriteAsJsonAsync(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled error occurred.");

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                var response = new ApiError
                {
                    Message = "Internal error.",
                    Timestamp = DateTime.UtcNow
                };
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}

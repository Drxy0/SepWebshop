using Microsoft.AspNetCore.Mvc;

namespace CardService.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                var problemDetails = new ProblemDetails();
                var statusCode = StatusCodes.Status500InternalServerError;
                
                switch (exception)
                {
                    case FluentValidation.ValidationException fluentEx:
                        statusCode = StatusCodes.Status422UnprocessableEntity;

                        problemDetails.Status = statusCode;
                        problemDetails.Type = "ValidationFailure";
                        problemDetails.Title = "Validation error";
                        problemDetails.Detail = fluentEx.Message;

                        problemDetails.Extensions["errors"] = fluentEx.Errors.Select(e =>
                            new { field = e.PropertyName, error = e.ErrorMessage });
                        break;

                    default:
                        problemDetails.Status = statusCode;
                        problemDetails.Type = "ServerError";
                        problemDetails.Title = "Server error";
                        problemDetails.Detail = exception.Message;
                        break;
                }

                context.Response.StatusCode = statusCode;
                await context.Response.WriteAsJsonAsync(problemDetails);
            }
        }
    }
}

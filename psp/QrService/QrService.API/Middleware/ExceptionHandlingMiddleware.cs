using Microsoft.AspNetCore.Mvc;
using QrService.Application.Common.Exceptions;

namespace QrService.API.Middleware
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
                    case ValidationException ex:
                        statusCode = StatusCodes.Status422UnprocessableEntity;

                        problemDetails.Status = StatusCodes.Status422UnprocessableEntity;
                        problemDetails.Type = "ValidationFailure";
                        problemDetails.Title = "Validation error";
                        problemDetails.Detail = "One or more validation errors has occurred.";

                        if (ex.Errors is not null)
                        {
                            problemDetails.Extensions["errors"] = ex.Errors;
                        }
                        break;

                    case FluentValidation.ValidationException fluentEx:
                        statusCode = StatusCodes.Status422UnprocessableEntity;

                        problemDetails.Status = statusCode;
                        problemDetails.Type = "ValidationFailure";
                        problemDetails.Title = "Validation error";
                        problemDetails.Detail = fluentEx.Message;

                        problemDetails.Extensions["errors"] = fluentEx.Errors.Select(e =>
                            new { field = e.PropertyName, error = e.ErrorMessage });
                        break;

                    case NotFoundException notFoundEx:
                        statusCode = StatusCodes.Status404NotFound;

                        problemDetails.Status = statusCode;
                        problemDetails.Type = "NotFound";
                        problemDetails.Title = "Entity not found";
                        problemDetails.Detail = notFoundEx.Message;
                        break;

                    default:

                        problemDetails.Status = statusCode;
                        problemDetails.Type = exception.GetType().Name;
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

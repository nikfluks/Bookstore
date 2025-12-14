using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.API.Middleware
{
    public class GlobalExceptionHandler() : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // log, send email, etc.

            var isDevelopment = httpContext.RequestServices
                .GetRequiredService<IHostEnvironment>()
                .IsDevelopment();

            var problemDetails = exception switch
            {
                ArgumentNullException argNullEx => new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Bad Request - Input was null",
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    Detail = isDevelopment ? argNullEx.ToString() : "A required parameter was not provided",
                    Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
                    Extensions = { ["traceId"] = httpContext.TraceIdentifier }
                },
                // Add more specific exception types as needed...
                _ => new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An error occurred while processing your request.",
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                    Detail = isDevelopment ? exception.ToString() : "An internal server error occurred",
                    Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
                    Extensions = { ["traceId"] = httpContext.TraceIdentifier }
                }
            };

            httpContext.Response.StatusCode = problemDetails.Status!.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}

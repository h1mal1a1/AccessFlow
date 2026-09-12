using AccessFlow.Application.Clients.Exceptions;
using AccessFlow.Application.Connections.Exceptions;
using AccessFlow.Application.VPS.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AccessFlow.Api.ExceptionHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ConnectionConflictException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
            await httpContext.Response.WriteAsJsonAsync(
                new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Connection conflict",
                    Detail = exception.Message
                },
                cancellationToken
            );
            return true;
        }

        if (exception is ClientNotFoundException or ConnectionNotFoundException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

            await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Resource not found",
                Detail = exception.Message
            },
            cancellationToken);

            return true;
        }

        if (exception is VpsException vpsException)
        {
            var statusCode = vpsException.ErrorType switch
            {
                VpsErrorType.Conflict => StatusCodes.Status409Conflict,
                VpsErrorType.NotFound => StatusCodes.Status404NotFound,
                VpsErrorType.InvalidResponse => StatusCodes.Status502BadGateway,
                VpsErrorType.OperationFailed => StatusCodes.Status502BadGateway,
                VpsErrorType.Unavailable => StatusCodes.Status503ServiceUnavailable,
                VpsErrorType.Configuration => StatusCodes.Status502BadGateway,
                _ => StatusCodes.Status500InternalServerError
            };

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(
                new ProblemDetails
                {
                    Status = statusCode,
                    Title = "VPS error",
                    Detail = vpsException.Message
                },
                cancellationToken);

            return true;
        }

        return false;
    }
}
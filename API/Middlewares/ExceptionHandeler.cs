using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    // Standard Dependency Injection works perfectly here
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> _logger)
    {
        this._logger = _logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1. Log the full error to your server monitoring system safely
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        // 2. Standardize your error response format using standard RFC 7807 Problem Details
        var problemDetails = new ProblemDetails
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Title = "Server Error",
            Detail = "An unexpected error occurred on the server. Please try again later.",
            Instance = httpContext.Request.Path
        };

        // 3. Set the response properties
        httpContext.Response.StatusCode = problemDetails.Status.Value;
        httpContext.Response.ContentType = "application/problem+json";

        // 4. Write the structured object out to the client response stream
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        // Return true to signal that this exception has been completely handled 
        // and the pipeline can safely short-circuit without crashing the process.
        return true;
    }
}

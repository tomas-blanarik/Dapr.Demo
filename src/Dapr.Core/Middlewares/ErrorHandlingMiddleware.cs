using Dapr.Core.Exceptions;
using Dapr.Core.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dapr.Core.Middlewares;

public class ErrorHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (DomainEntityNotFoundException denfex)
        {
            LogException(denfex);
            await WriteErrorResponseAsync(context, StatusCodes.Status404NotFound, denfex);
        }
        catch (UnexpectedErrorException ueex)
        {
            LogException(ueex);
            await WriteErrorResponseAsync(context, StatusCodes.Status400BadRequest, ueex);
        }
        catch (ConflictException cex)
        {
            LogException(cex);
            await WriteErrorResponseAsync(context, StatusCodes.Status409Conflict, cex);
        }
        catch (Exception ex)
        {
            LogException(ex);
            await WriteErrorResponseAsync(context, StatusCodes.Status500InternalServerError, ex);
        }
    }

    private async static Task WriteErrorResponseAsync(HttpContext context, int statusCode, Exception ex)
    {
        var error = new ErrorResponse
        {
            StatusCode = statusCode,
            Message = ex.Message,
            DeveloperMessage = ex.StackTrace
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(error);
    }

    private void LogException(Exception ex)
    {
        _logger.LogError(ex, ex.Message);
        Exception? inner = ex.InnerException;
        while (inner != null)
        {
            _logger.LogError(inner, inner.Message);
            inner = inner.InnerException;
        }
    }
}

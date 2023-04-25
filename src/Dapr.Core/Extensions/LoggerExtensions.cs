using Dapr.Client;
using Dapr.Core.Responses;
using Microsoft.Extensions.Logging;

namespace Dapr.Core.Extensions;

public static class LoggerExtensions
{
    public static void LogDaprInvocationError(this ILogger logger, InvocationException exception, ErrorResponse? response = null)
    {
        var httpResponse = exception.Response;
        logger.LogError("Error occurred while invocating {InvocationUrl}: {ExceptionMessage}",
            exception.MethodName, response?.Message ?? httpResponse.ReasonPhrase);
    }
}

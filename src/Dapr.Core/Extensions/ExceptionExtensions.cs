using System.Net.Http.Json;
using Dapr.Client;
using Dapr.Core.Responses;

namespace Dapr.Core.Extensions;

public static class ExceptionExtensions
{
    public static bool TryParseErrorResponse(this InvocationException ex, out ErrorResponse? error)
    {
        error = default;
        var response = ex.Response;
        if (response is null)
        {
            return false;
        }

        try
        {
            error = response.Content.ReadFromJsonAsync<ErrorResponse>().Result;
        }
        catch
        {
            // ignore
        }

        return error is not null;
    }
}

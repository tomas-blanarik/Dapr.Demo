using Dapr.Core.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dapr.Core.Extensions;

public static class ControllerExtensions
{
    public static IActionResult GetResultFromDaprErrorResponse(this ControllerBase controller, ErrorResponse response)
    {
        return response.StatusCode switch
        {
            StatusCodes.Status400BadRequest => controller.BadRequest(response),
            StatusCodes.Status404NotFound => controller.NotFound(response),
            StatusCodes.Status409Conflict => controller.Conflict(response),
            _ => controller.StatusCode(500, response)
        };
    }
}

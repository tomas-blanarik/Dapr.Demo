namespace Dapr.Core.Responses;

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = null!;
    public string? DeveloperMessage { get; set; }
}

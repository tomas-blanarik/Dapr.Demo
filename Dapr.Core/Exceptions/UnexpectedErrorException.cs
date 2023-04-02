namespace Dapr.Core.Exceptions;

public class UnexpectedErrorException : Exception
{
    public UnexpectedErrorException(string message) : base(message)
    { }
}

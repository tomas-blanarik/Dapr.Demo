namespace Dapr.Core.Exceptions;

public class UnexpectedErrorException(string message) : Exception(message)
{
}

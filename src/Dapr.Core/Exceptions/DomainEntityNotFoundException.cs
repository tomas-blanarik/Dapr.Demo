namespace Dapr.Core.Exceptions;

public class DomainEntityNotFoundException(Type type, Guid id) : Exception(string.Format(_format, type.Name, id))
{
    private const string _format = "${0} with entity ID '{1}' not found";
}


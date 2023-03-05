namespace Dapr.Core.Exceptions;

public class DomainEntityNotFoundException : Exception
{
    private const string _format = "${0} with entity ID '{1}' not found";

    public DomainEntityNotFoundException(Type type, Guid id)
        : base(string.Format(_format, type.Name, id))
    { }
}


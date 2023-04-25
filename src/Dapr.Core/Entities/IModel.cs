namespace Dapr.Core.Entities;

public interface IModel<T> where T : IDomainEntity
{
    T ToDomain();
}

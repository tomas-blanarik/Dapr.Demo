namespace Dapr.Core.Entities;

public interface IDomainEntity
{
    Guid? EntityId { get; }
}

public interface IDomainEntity<TDTO> : IDomainEntity
{
    TDTO ToDTO();
}

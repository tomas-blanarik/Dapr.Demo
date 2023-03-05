using Dapr.Core.Paging;

namespace Dapr.Ordering.Api.Queries;

public class ListOrdersQuery : PagingRequest
{
    public Guid? UserId { get; set; }
}

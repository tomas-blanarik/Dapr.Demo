using Dapr.Core.Paging;

namespace Dapr.Audit.Api.Queries;

public class ListAuditItemsQuery : PagingRequest
{
    public Guid? UserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? EventType { get; set; }
    public bool? OrderByDate { get; set; } = true;
}

using System.ComponentModel.DataAnnotations;

namespace Dapr.Core.Paging;

public class PagingRequest
{
    [Range(1, int.MaxValue)]
    public int PageIndex { get; set; } = 1;

    [Range(1, int.MaxValue)]
    public int PageSize { get; set; } = 50;
}

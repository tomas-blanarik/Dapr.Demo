namespace Dapr.Core.Paging;

public class PagedResult<T>
{
    public PagingRequest Paging { get; set; } = null!;
    public IEnumerable<T> Items { get; set; } = null!;

    public PagedResult<TDTO> ConvertToDTO<TDTO>(Func<T, TDTO> converter) => new()
    {
        Paging = Paging,
        Items = Items.Select(converter)
    };
}
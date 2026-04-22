namespace CashControl.Core.Application;

public class PagedQueryResultPagination : IPagedQueryResultPagination
{
    public uint TotalItems { get; set; }

    public uint PageSize { get; set; }

    public uint PageNumber { get; set; }
}
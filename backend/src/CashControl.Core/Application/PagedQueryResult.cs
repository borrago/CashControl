namespace CashControl.Core.Application;

public class PagedQueryResult : ItemsQueryResult
{
    public IPagedQueryResultPagination? Pagination { get; set; }
}
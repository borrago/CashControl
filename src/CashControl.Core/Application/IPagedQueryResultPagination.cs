namespace CashControl.Core.Application;

public interface IPagedQueryResultPagination
{
    uint TotalItems { get; set; }

    uint PageSize { get; set; }

    uint PageNumber { get; set; }
}
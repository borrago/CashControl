namespace CashControl.Core.Application;

public class PagedQueryConfiguration : IPagedQueryConfiguration
{
    public string SortField { get; set; } = string.Empty;

    public string SortOrder { get; set; } = "ASC";

    public uint PageSize { get; set; } = 10;

    public uint PageNumber { get; set; } = 1;
}
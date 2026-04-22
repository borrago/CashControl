namespace CashControl.Core.Application;

public interface IPagedQueryConfiguration
{
    uint PageSize { get; set; }

    uint PageNumber { get; set; }
}
namespace CashControl.Core.API;

public abstract class PaginationSwagger
{
    public uint TotalItems { get; set; }
    public uint PageSize { get; set; }
    public uint PageNumber { get; set; }
}

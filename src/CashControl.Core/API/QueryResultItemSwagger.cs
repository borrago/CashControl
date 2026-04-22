namespace CashControl.Core.API;

public abstract class QueryResultItemSwagger<T>
{
    public PaginationSwagger? Pagination { get; set; }
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
}

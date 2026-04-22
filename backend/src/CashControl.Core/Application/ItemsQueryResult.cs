namespace CashControl.Core.Application;

public class ItemsQueryResult : QueryResult
{
    public IEnumerable<IQueryResultItem>? Items { get; set; }
}
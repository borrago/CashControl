namespace CashControl.Core.Application;

public class QueryInput<TQueryResult> : PagedQueryInput<TQueryResult> where TQueryResult : QueryResult;
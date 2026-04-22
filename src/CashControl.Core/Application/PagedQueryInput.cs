using Microsoft.AspNetCore.Mvc;

namespace CashControl.Core.Application;

public class PagedQueryInput<TQueryResult> : MediatorInput<TQueryResult> where TQueryResult : QueryResult
{
    [FromQuery]
    public string SortField { get; set; } = string.Empty;
    [FromQuery]
    public string SortOrder { get; set; } = string.Empty;
    [FromQuery]
    public uint PageSize { get; set; }
    [FromQuery]
    public uint PageNumber { get; set; }
    [FromQuery]
    public string Fields { get; set; } = string.Empty; // Caso alterar esse nome, alterar em BaseController.OnActionExecuting
}
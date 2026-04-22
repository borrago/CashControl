using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CashControl.Core.Application;

/// <summary>
/// Conjunto de Extensions do Core.
/// </summary>
public static class AppExtensions
{
    private const uint PageNumberDefault = 1;
    private const uint PageSizeDefault = 10;

    /// <summary>
    /// Método para retornar resultado com paginação.
    /// </summary>
    /// <param name="queryItems">Itens a paginar</param>
    /// <param name="queryInputPaged">Input da paginação</param>
    /// <returns></returns>
    public static async Task<PagedQueryResult> PaginateAsync(this IQueryable<IQueryResultItem> queryItems, PagedQueryInput<PagedQueryResult> queryInputPaged, CancellationToken cancellationToken = default)
    {
        var items = await queryItems.ToListAsync(cancellationToken);

        if (queryInputPaged == null)
            return new PagedQueryResult { Items = items };

        if (queryInputPaged.PageNumber == uint.MinValue)
            queryInputPaged.PageNumber = PageNumberDefault;

        if (queryInputPaged?.PageSize == uint.MinValue)
            queryInputPaged.PageSize = PageSizeDefault;

        var countMax = (uint)(items.Count);

        var pagedItems = items
            .Skip(Convert.ToInt32((queryInputPaged?.PageNumber) - 1) * Convert.ToInt32(queryInputPaged?.PageSize))
            .Take(Convert.ToInt32(queryInputPaged?.PageSize));

        return new PagedQueryResult
        {
            Items = pagedItems,
            Pagination = new PagedQueryResultPagination
            {
                TotalItems = countMax,
                PageSize = Convert.ToUInt32(queryInputPaged?.PageSize),
                PageNumber = Convert.ToUInt32(queryInputPaged?.PageNumber)
            }
        };
    }

    public static async Task<T> HandleResponseAsync<T>(this HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
        var retorno = JsonConvert.DeserializeObject<T>(jsonString);

        if (retorno is null)
            throw new ApplicationException("Falha ao deserializar objeto.");

        return retorno;
    }
}

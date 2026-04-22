using CashControl.Core.Application;
using Microsoft.AspNetCore.Mvc;
using System.Dynamic;
using System.Net;
using System.Reflection;

namespace CashControl.Core.API;

public static class BaseControllerHelper
{
    public static IActionResult HandleMediatorResult(this IMediatorResult mediatorResult, string location = "", bool isSystemicUser = false, string camposRetorno = "")
    {
        if (mediatorResult == null)
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);

        if (!mediatorResult.IsValid())
            return new BadRequestObjectResult(ApiErrorResponse.Validation(mediatorResult.Errors));

        if (mediatorResult.HttpStatusCode == HttpStatusCode.NoContent)
            return new NoContentResult();

        var result = new ExpandoObject() as IDictionary<string, object>;

        foreach (var property in mediatorResult.GetType().GetProperties()
            .Where(w => w.Name != nameof(mediatorResult.HttpStatusCode) &&
            w.Name != nameof(mediatorResult.Errors)))
        {
            if (property.Name == nameof(PagedQueryResult.Pagination))
            {
                result.Add(property.Name, property.GetValue(mediatorResult) ?? "");
                continue;
            }

            var obj = MaskPropertyIfSystemicUserFromResult(property, isSystemicUser, mediatorResult, camposRetorno);
            if (obj == null)
                continue;

            result.Add(property.Name, obj);
        }

        if (mediatorResult.HttpStatusCode != null)
        {
            if (mediatorResult.HttpStatusCode == HttpStatusCode.Created)
                return new CreatedResult(location, result);

            return new ObjectResult(result)
            {
                StatusCode = (int)mediatorResult.HttpStatusCode
            };
        }

        return result.Any() ? new OkObjectResult(result) : new OkResult();
    }

    private static object? MaskPropertyIfSystemicUserFromResult(PropertyInfo property, bool isSystemicUser, IMediatorResult mediatorResult, string camposRetorno)
    {
        var propertyValue = property.GetValue(mediatorResult);
        if (propertyValue == null)
            return null;

        if (propertyValue is IEnumerable<object> enumerable && propertyValue.GetType().IsGenericType)
        {
            if (IsSimpleEnumerable(enumerable))
                return enumerable.ToList();

            var obj = new List<Dictionary<string, object>>();
            foreach (var item in enumerable)
                obj.Add(FiltrarCampos(isSystemicUser, camposRetorno, item));

            return obj;
        }

        if (string.IsNullOrEmpty(camposRetorno) || camposRetorno.ToLower().Contains(property.Name.ToLower()))
            return propertyValue;

        return null;
    }

    private static bool IsSimpleEnumerable(IEnumerable<object> enumerable)
    {
        foreach (var item in enumerable)
        {
            if (item is null)
                continue;

            var itemType = item.GetType();
            if (itemType.IsPrimitive || item is string || item is decimal || item is DateTime || item is Guid)
                continue;

            return false;
        }

        return true;
    }

    private static Dictionary<string, object> FiltrarCampos(bool isSystemicUser, string camposRetorno, object item)
    {
        var dictionary = new Dictionary<string, object>();

        foreach (var propertyItem in item.GetType().GetProperties()
            .Where(w => (!string.IsNullOrEmpty(camposRetorno) && camposRetorno.ToLower().Contains(w.Name.ToLower())) ||
            string.IsNullOrEmpty(camposRetorno)))
        {
            dictionary.Add(propertyItem.Name, propertyItem.GetValue(item) ?? "");
        }

        return dictionary;
    }
}

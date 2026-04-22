using CashControl.Core.CrossCutting;
using Newtonsoft.Json;
using System.Net;

namespace CashControl.Core.Application;

public class MediatorResult : IMediatorResult
{
    public MediatorResult() { }

    public MediatorResult(HttpStatusCode httpStatusCode)
    {
        WithHttpStatusCode(httpStatusCode);
    }

    public MediatorResult(HttpStatusCode httpStatusCode, IEnumerable<CustomValidationFailure> errors)
    {
        WithHttpStatusCode(httpStatusCode);
        WithErrors(errors);
    }

    public IMediatorResult WithHttpStatusCode(HttpStatusCode httpStatusCode)
    {
        HttpStatusCode = httpStatusCode;
        return this;
    }

    public IMediatorResult WithErrors(IEnumerable<CustomValidationFailure> errors)
    {
        Errors = errors;
        return this;
    }

    public bool IsValid()
    {
        return !Errors.Any();
    }

    [JsonIgnore]
    public HttpStatusCode? HttpStatusCode { get; set; }

    [JsonIgnore]
    public IEnumerable<CustomValidationFailure> Errors { get; set; } = new List<CustomValidationFailure>();

}
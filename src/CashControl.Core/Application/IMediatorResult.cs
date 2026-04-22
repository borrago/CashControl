using CashControl.Core.CrossCutting;
using System.Net;

namespace CashControl.Core.Application;

public interface IMediatorResult
{
    HttpStatusCode? HttpStatusCode { get; set; }

    IEnumerable<CustomValidationFailure> Errors { get; set; }

    IMediatorResult WithErrors(IEnumerable<CustomValidationFailure> errors);

    IMediatorResult WithHttpStatusCode(HttpStatusCode httpStatusCode);

    bool IsValid();
}
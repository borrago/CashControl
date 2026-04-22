using CashControl.Core.CrossCutting;

namespace CashControl.Core.Application;

[Serializable]
public class ApplicationException : CustomException
{
    public ApplicationException() { }

    public ApplicationException(string message) : base(message) { }

    public ApplicationException(IEnumerable<CustomValidationFailure> errors) : base(errors) { }

    public ApplicationException(string message, Exception innerException) : base(message, innerException) { }
}
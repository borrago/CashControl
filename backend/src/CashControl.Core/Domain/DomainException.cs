using CashControl.Core.CrossCutting;

namespace CashControl.Core.Domain;

[Serializable]
public class DomainException : CustomException
{
    public DomainException() { }

    public DomainException(string message) : base(message) { }

    public DomainException(IEnumerable<CustomValidationFailure> errors) : base(errors) { }
}
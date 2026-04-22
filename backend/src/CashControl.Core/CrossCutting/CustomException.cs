namespace CashControl.Core.CrossCutting;

[Serializable]
public class CustomException : Exception
{
    public IEnumerable<CustomValidationFailure> Errors { get; private set; } = default!;

    public void SetErrors(IEnumerable<CustomValidationFailure> erros) => Errors = erros;

    public CustomException() { }

    public CustomException(IEnumerable<CustomValidationFailure> errors) { Errors = errors; }

    public CustomException(string message, Exception innerException) : base(message, innerException) { }

    public CustomException(string message) : base(message)
        => Errors = new List<CustomValidationFailure>()
        {
               new CustomValidationFailure(message)
        };
}
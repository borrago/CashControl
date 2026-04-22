namespace CashControl.Core.CrossCutting;

[Serializable]
public class CustomValidationFailure
{
    public CustomValidationFailure() { }

    public CustomValidationFailure(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }

    public CustomValidationFailure(string propertyName, string errorMessage)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
    }

    public string PropertyName { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;
}
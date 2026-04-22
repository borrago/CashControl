namespace CashControl.Core.CrossCutting;

public abstract class AbstractValidator : IValidator
{
    protected readonly IValidationHelper<IValidator> Validator;

    protected AbstractValidator()
    {
        Validator = new ValidationHelper<IValidator>();
    }

    private IEnumerable<CustomValidationFailure> GetErros() => Validator.Validade(GetInstance());

    protected abstract IValidator GetInstance();
    protected abstract CustomException GetCustomException();

    public void Validate()
    {
        var errors = GetErros();
        if (errors.Any())
        {
            var exception = GetCustomException();
            exception.SetErrors(errors);

            throw exception;
        }
    }
}
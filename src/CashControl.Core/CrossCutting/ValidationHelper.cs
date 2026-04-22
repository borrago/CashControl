using FluentValidation;
using System.Globalization;
using System.Linq.Expressions;

namespace CashControl.Core.CrossCutting;

public class ValidationHelper<TClass> : AbstractValidator<TClass>, IValidationHelper<TClass>
{
    public ValidationHelper()
    {
        ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("pt-BR");
    }

    public void AssertNotNullOrEmpty<TProperty>(Expression<Func<TClass, TProperty>> expression, string? propertyName = null)
        => RuleFor(expression).NotNull().NotEmpty()
        .WithMessage(string.Format($"'{propertyName ?? ((MemberExpression)expression.Body).Member.Name}' não pode ser nulo ou vazio."));

    public void AssertFalse(Expression<Func<TClass, bool>> expression, string? propertyName = null)
        => RuleFor(expression).Equal(false).WithMessage($"'{propertyName ?? ((MemberExpression)expression.Body).Member.Name}' deve ser falso.");

    public void AssertTrue(Expression<Func<TClass, bool>> expression, string? propertyName = null)
        => RuleFor(expression).Equal(true).WithMessage($"'{propertyName ?? ((MemberExpression)expression.Body).Member.Name}' deve ser verdadeiro.");

    public void AssertEquals<TValue>(Expression<Func<TClass, TValue>> expression, TValue toCompare, string? propertyName = null) where TValue : IComparable<TValue>, IComparable, IEquatable<TValue>
        => RuleFor(expression).Equal(toCompare).WithName(propertyName ?? ((MemberExpression)expression.Body).Member.Name);

    public void AssertNotEquals<TValue>(Expression<Func<TClass, TValue>> expression, TValue toCompare, string? propertyName = null) where TValue : IComparable<TValue>, IComparable, IEquatable<TValue>
        => RuleFor(expression).NotEqual(toCompare).WithName(propertyName ?? ((MemberExpression)expression.Body).Member.Name);

    public void AssertEquals(Expression<Func<TClass, Enum>> expression, Enum toCompare, string propertyName)
        => RuleFor(expression).Equal(toCompare).WithName(propertyName);

    public void AssertNotEquals(Expression<Func<TClass, Enum>> expression, Enum toCompare, string propertyName)
        => RuleFor(expression).NotEqual(toCompare).WithName(propertyName);

    public void AssertMatches(Expression<Func<TClass, string>> expression, string pattern, string? propertyName = null)
        => RuleFor(expression).Matches(pattern).WithName(propertyName ?? ((MemberExpression)expression.Body).Member.Name);

    public void AssertEmail(Expression<Func<TClass, string>> expression, string? propertyName = null)
        => RuleFor(expression).EmailAddress().WithName(propertyName ?? ((MemberExpression)expression.Body).Member.Name);

    public void AssertMinimumLength(Expression<Func<TClass, string>> expression, int minimum, string? propertyName = null)
        => RuleFor(expression).MinimumLength(minimum).WithName(propertyName ?? ((MemberExpression)expression.Body).Member.Name);

    public void AssertMaximumLength(Expression<Func<TClass, string>> expression, int maximum, string? propertyName = null)
        => RuleFor(expression).MaximumLength(maximum).WithName(propertyName ?? ((MemberExpression)expression.Body).Member.Name);

    public void AssertLength(Expression<Func<TClass, string>> expression, int minimum, int maximum, string? propertyName = null)
        => RuleFor(expression).MinimumLength(minimum).MaximumLength(maximum).WithName(propertyName ?? ((MemberExpression)expression.Body).Member.Name);

    public void AssertLength(Expression<Func<TClass, string>> expression, int exactLength, string? propertyName = null)
        => RuleFor(expression).Length(exactLength)
        .WithMessage(string.Format($"'{propertyName ?? ((MemberExpression)expression.Body).Member.Name}' deve conter exatamente {exactLength} caracter(es)."));

    public void AssertRange(Expression<Func<TClass, double>> expression, double minimum, double maximum, string? propertyName = null)
        => RuleFor(expression).GreaterThanOrEqualTo(minimum).LessThanOrEqualTo(maximum).WithName(propertyName ?? ((MemberExpression)expression.Body).Member.Name);

    public void AssertRange(Expression<Func<TClass, float>> expression, float minimum, float maximum, string? propertyName = null)
        => RuleFor(expression).GreaterThanOrEqualTo(minimum).LessThanOrEqualTo(maximum).WithName(propertyName ?? ((MemberExpression)expression.Body).Member.Name);

    public void AssertRange(Expression<Func<TClass, int>> expression, int minimum, int maximum, string? propertyName = null)
        => RuleFor(expression).GreaterThanOrEqualTo(minimum).LessThanOrEqualTo(maximum).WithName(propertyName ?? ((MemberExpression)expression.Body).Member.Name);

    public void AssertRange(Expression<Func<TClass, long>> expression, long minimum, long maximum, string? propertyName = null)
        => RuleFor(expression).GreaterThanOrEqualTo(minimum).LessThanOrEqualTo(maximum).WithName(propertyName ?? ((MemberExpression)expression.Body).Member.Name);

    public void AssertRange(Expression<Func<TClass, decimal>> expression, decimal minimum, decimal maximum, string? propertyName = null)
        => RuleFor(expression).GreaterThanOrEqualTo(minimum).LessThanOrEqualTo(maximum).WithName(propertyName ?? ((MemberExpression)expression.Body).Member.Name);

    public void LessThan<TValue>(Expression<Func<TClass, TValue>> expression, TValue valueToCompare, string? propertyName = null) where TValue : IComparable<TValue>, IComparable
        => RuleFor(expression).LessThan(valueToCompare).WithName(propertyName ?? ((MemberExpression)expression.Body).Member.Name);

    public void LessThanOrEqualTo<TValue>(Expression<Func<TClass, TValue>> expression, TValue valueToCompare, string? propertyName = null) where TValue : IComparable<TValue>, IComparable
        => RuleFor(expression).LessThanOrEqualTo(valueToCompare).WithName(propertyName ?? ((MemberExpression)expression.Body).Member.Name);

    public void GreaterThan<TValue>(Expression<Func<TClass, TValue>> expression, TValue valueToCompare, string? propertyName = null) where TValue : IComparable<TValue>, IComparable
        => RuleFor(expression).GreaterThan(valueToCompare).WithName(propertyName ?? ((MemberExpression)expression.Body).Member.Name);

    public void GreaterThanOrEqualTo<TValue>(Expression<Func<TClass, TValue>> expression, TValue valueToCompare, string? propertyName = null) where TValue : IComparable<TValue>, IComparable
        => RuleFor(expression).GreaterThanOrEqualTo(valueToCompare).WithName(propertyName ?? ((MemberExpression)expression.Body).Member.Name);

    public void IsInEnum<TEnum>(Expression<Func<TClass, TEnum>> expression, string? propertyName = null)
        => RuleFor(expression).NotNull().IsInEnum().WithName(propertyName ?? ((MemberExpression)expression.Body).Member.Name);

    public void IsCellPhone(Expression<Func<TClass, string>> expression, string? propertyName = null)
        => RuleFor(expression).Must(s => (s != null) && s.Substring(2, 1) == "9").WithMessage($"'{propertyName ?? ((MemberExpression)expression.Body).Member.Name}' não é um telefone celular válido.");

    public IEnumerable<CustomValidationFailure> Validade(TClass domain)
    {
        var result = Validate(domain);

        if (!result.IsValid)
            return result.Errors.Select(x => new CustomValidationFailure
            (
                x.PropertyName,
                x.ErrorMessage
            ));

        return [];
    }
}
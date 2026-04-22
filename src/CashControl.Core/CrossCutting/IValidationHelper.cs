using System.Linq.Expressions;

namespace CashControl.Core.CrossCutting;

public interface IValidationHelper<TClass>
{
    public void AssertNotNullOrEmpty<TProperty>(Expression<Func<TClass, TProperty>> expression, string? propertyName = null);

    public void AssertFalse(Expression<Func<TClass, bool>> expression, string? propertyName = null);

    public void AssertTrue(Expression<Func<TClass, bool>> expression, string? propertyName = null);

    public void AssertEquals<TValue>(Expression<Func<TClass, TValue>> expression, TValue toCompare, string? propertyName = null) where TValue : IComparable<TValue>, IComparable, IEquatable<TValue>;

    public void AssertNotEquals<TValue>(Expression<Func<TClass, TValue>> expression, TValue toCompare, string? propertyName = null) where TValue : IComparable<TValue>, IComparable, IEquatable<TValue>;

    public void AssertEquals(Expression<Func<TClass, Enum>> expression, Enum toCompare, string propertyName);

    public void AssertNotEquals(Expression<Func<TClass, Enum>> expression, Enum toCompare, string propertyName);

    public void AssertMatches(Expression<Func<TClass, string>> expression, string pattern, string? propertyName = null);

    public void AssertEmail(Expression<Func<TClass, string>> expression, string? propertyName = null);

    public void AssertMinimumLength(Expression<Func<TClass, string>> expression, int minimum, string? propertyName = null);

    public void AssertMaximumLength(Expression<Func<TClass, string>> expression, int maximum, string? propertyName = null);

    public void AssertLength(Expression<Func<TClass, string>> expression, int minimum, int maximum, string? propertyName = null);

    public void AssertLength(Expression<Func<TClass, string>> expression, int exactLength, string? propertyName = null);

    public void AssertRange(Expression<Func<TClass, double>> expression, double minimum, double maximum, string? propertyName = null);

    public void AssertRange(Expression<Func<TClass, float>> expression, float minimum, float maximum, string? propertyName = null);

    public void AssertRange(Expression<Func<TClass, int>> expression, int minimum, int maximum, string? propertyName = null);

    public void AssertRange(Expression<Func<TClass, long>> expression, long minimum, long maximum, string? propertyName = null);

    public void AssertRange(Expression<Func<TClass, decimal>> expression, decimal minimum, decimal maximum, string? propertyName = null);

    public void LessThan<TValue>(Expression<Func<TClass, TValue>> expression, TValue valueToCompare, string? propertyName = null) where TValue : IComparable<TValue>, IComparable;

    public void LessThanOrEqualTo<TValue>(Expression<Func<TClass, TValue>> expression, TValue valueToCompare, string? propertyName = null) where TValue : IComparable<TValue>, IComparable;

    public void GreaterThan<TValue>(Expression<Func<TClass, TValue>> expression, TValue valueToCompare, string? propertyName = null) where TValue : IComparable<TValue>, IComparable;

    public void GreaterThanOrEqualTo<TValue>(Expression<Func<TClass, TValue>> expression, TValue valueToCompare, string? propertyName = null) where TValue : IComparable<TValue>, IComparable;

    public void IsInEnum<TEnum>(Expression<Func<TClass, TEnum>> expression, string? propertyName = null);

    public void IsCellPhone(Expression<Func<TClass, string>> expression, string? propertyName = null);

    public IEnumerable<CustomValidationFailure> Validade(TClass domain);
}
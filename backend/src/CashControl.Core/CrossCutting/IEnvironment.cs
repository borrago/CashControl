namespace CashControl.Core.CrossCutting;

public interface IEnvironment
{
    string Name { get; }
    IEnumerable<KeyValuePair<string, string?>>? Parameters { get; }
    string this[string key] { get; }

    bool IsDevelopment();

    bool IsProduction();

    bool IsUat();
}
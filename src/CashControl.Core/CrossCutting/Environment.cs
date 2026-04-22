namespace CashControl.Core.CrossCutting;

public class Environment : IEnvironment
{
    private const string DevelopmentEnvironmentName = "Development";
    private const string ProductionEnvironmentName = "Production";
    private const string UatEnvironmentName = "Uat";

    public Environment(string name, IEnumerable<KeyValuePair<string, string?>>? parameters = default)
    {
        Name = name.Trim();
        Parameters = parameters;
    }

    public IEnumerable<KeyValuePair<string, string?>>? Parameters { get; }

    public bool IsDevelopment()
    {
        return string.Equals(Name.Trim(), DevelopmentEnvironmentName, StringComparison.CurrentCultureIgnoreCase);
    }

    public bool IsProduction()
    {
        return string.Equals(Name.Trim(), ProductionEnvironmentName, StringComparison.CurrentCultureIgnoreCase);
    }

    public bool IsUat()
    {
        return string.Equals(Name.Trim(), UatEnvironmentName, StringComparison.CurrentCultureIgnoreCase);
    }

    public string Name { get; } = string.Empty;

    public string this[string key]
    {
        get { return Parameters?.FirstOrDefault(x => x.Key == key).Value ?? ""; }
    }
}
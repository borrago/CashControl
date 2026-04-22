namespace CashControl.Core.CrossCutting;

public class Tenant
{
    public int Id { get; set; }
    public string ConnectionString { get; set; } = string.Empty;
}

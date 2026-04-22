namespace CashControl.Core.Infra.DataSeeding;

public interface IDatabaseSeeder
{
    Task SeedAsync(CancellationToken cancellationToken);
}
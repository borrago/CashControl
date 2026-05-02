namespace CashControl.Core.CrossCutting;

public class CorsOptionsSettings
{
    public string[] AllowedOrigins { get; set; } = [];
    public bool AllowCredentials { get; set; } = true;
}

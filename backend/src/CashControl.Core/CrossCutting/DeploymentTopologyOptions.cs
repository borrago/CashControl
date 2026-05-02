namespace CashControl.Core.CrossCutting;

public class DeploymentTopologyOptions
{
    public string? PublicAppOrigin { get; set; }
    public string? PublicApiOrigin { get; set; }
    public bool RequireHttps { get; set; } = true;
}

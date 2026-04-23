namespace CashControl.Core.CrossCutting;

public static class CustomClaimTypes
{
    public const string Tenant = "Tenant";
    public const string IsSuperUser = "is_super_user";
    public const string IsImpersonating = "is_impersonating";
    public const string ImpersonatedByUserId = "impersonated_by_user_id";
    public const string ImpersonatedByEmail = "impersonated_by_email";
}

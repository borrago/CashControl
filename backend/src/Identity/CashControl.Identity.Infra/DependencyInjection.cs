using CashControl.Core.CrossCutting;
using CashControl.Identity.Application.Services;
using CashControl.Identity.Domain.Entities;
using CashControl.Identity.Infra.Options;
using CashControl.Identity.Infra.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CashControl.Identity.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, CoreSettings settings)
    {
        var securityOptions = settings.Configuration!.GetSection("Security").Get<SecurityOptions>() ?? new SecurityOptions();

        services.AddDbContext<Context>(options =>
            options.UseSqlServer(settings.Configuration!.GetConnectionString(settings.ConnectionStringName)));

        services
            .AddIdentityCore<User>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.SignIn.RequireConfirmedEmail = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = securityOptions.MaxFailedAccessAttempts;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(securityOptions.DefaultLockoutMinutes);
            })
            .AddRoles<IdentityRole>()
            .AddSignInManager<SignInManager<User>>()
            .AddEntityFrameworkStores<Context>()
            .AddDefaultTokenProviders();

        services.AddScoped<IUserClaimsPrincipalFactory<User>, CashControlUserClaimsPrincipalFactory>();
        services.Configure<EmailOptions>(settings.Configuration!.GetSection("Email"));
        services.Configure<SecurityOptions>(settings.Configuration!.GetSection("Security"));
        services.AddScoped<IIdentityEmailSender, LoggingIdentityEmailSender>();

        services.AddScoped<IIdentityService, IdentityService>();
        services.RegisterCore(settings);

        return services;
    }
}

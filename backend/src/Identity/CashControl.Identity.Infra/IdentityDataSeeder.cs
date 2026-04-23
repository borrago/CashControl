using CashControl.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CashControl.Identity.Infra;

public static class IdentityDataSeeder
{
    public static async Task SeedIdentityDataAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        await EnsureRoleAsync(roleManager, IdentitySeedOptions.AdminRole);
        await EnsureRoleAsync(roleManager, IdentitySeedOptions.SuperAdminRole);

        var superUser = await userManager.FindByEmailAsync(IdentitySeedOptions.SuperAdminEmail);
        if (superUser is null)
        {
            superUser = new User
            {
                UserName = IdentitySeedOptions.SuperAdminEmail,
                Email = IdentitySeedOptions.SuperAdminEmail,
                EmailConfirmed = true,
                FullName = IdentitySeedOptions.SuperAdminFullName,
                Tenant = User.DefaultTenant,
                IsSuperUser = true
            };

            var createResult = await userManager.CreateAsync(superUser, IdentitySeedOptions.SuperAdminPassword);
            if (!createResult.Succeeded)
                throw new InvalidOperationException($"Falha ao criar super usuario: {string.Join(", ", createResult.Errors.Select(x => x.Description))}");
        }

        if (!superUser.EmailConfirmed || !superUser.IsSuperUser || superUser.Tenant != User.DefaultTenant)
        {
            superUser.EmailConfirmed = true;
            superUser.IsSuperUser = true;
            superUser.Tenant = User.DefaultTenant;

            var updateResult = await userManager.UpdateAsync(superUser);
            if (!updateResult.Succeeded)
                throw new InvalidOperationException($"Falha ao atualizar super usuario: {string.Join(", ", updateResult.Errors.Select(x => x.Description))}");
        }

        await EnsureUserRoleAsync(userManager, superUser, IdentitySeedOptions.AdminRole);
        await EnsureUserRoleAsync(userManager, superUser, IdentitySeedOptions.SuperAdminRole);
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string role)
    {
        if (await roleManager.RoleExistsAsync(role))
            return;

        var result = await roleManager.CreateAsync(new IdentityRole(role));
        if (!result.Succeeded)
            throw new InvalidOperationException($"Falha ao criar role '{role}': {string.Join(", ", result.Errors.Select(x => x.Description))}");
    }

    private static async Task EnsureUserRoleAsync(UserManager<User> userManager, User user, string role)
    {
        if (await userManager.IsInRoleAsync(user, role))
            return;

        var result = await userManager.AddToRoleAsync(user, role);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Falha ao associar role '{role}' ao usuario '{user.Email}': {string.Join(", ", result.Errors.Select(x => x.Description))}");
    }
}

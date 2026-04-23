using CashControl.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CashControl.Identity.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantSupportAndSuperUser : Migration
    {
        private const string AdminRoleId = "1f92dc4e-4481-441f-b016-2ffce4c3d101";
        private const string SuperAdminRoleId = "d822d7a7-2c2b-4d0e-8534-0e6f0d5ad201";
        private const string SuperAdminUserId = "8e48e3c8-303c-4f18-870e-4d87365c8301";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSuperUser",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Tenant",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: User.DefaultTenant);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Tenant",
                table: "AspNetUsers",
                column: "Tenant");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: ["Id", "Name", "NormalizedName", "ConcurrencyStamp"],
                values: new object[,]
                {
                    { AdminRoleId, IdentitySeedOptions.AdminRole, IdentitySeedOptions.AdminRole.ToUpperInvariant(), Guid.NewGuid().ToString() },
                    { SuperAdminRoleId, IdentitySeedOptions.SuperAdminRole, IdentitySeedOptions.SuperAdminRole.ToUpperInvariant(), Guid.NewGuid().ToString() }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns:
                [
                    "Id", "FullName", "RefreshToken", "RefreshTokenExpiryTimeUtc", "UserName", "NormalizedUserName",
                    "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
                    "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled",
                    "AccessFailedCount", "Tenant", "IsSuperUser"
                ],
                values: new object[]
                {
                    SuperAdminUserId,
                    IdentitySeedOptions.SuperAdminFullName,
                    null,
                    null,
                    IdentitySeedOptions.SuperAdminEmail,
                    IdentitySeedOptions.SuperAdminEmail.ToUpperInvariant(),
                    IdentitySeedOptions.SuperAdminEmail,
                    IdentitySeedOptions.SuperAdminEmail.ToUpperInvariant(),
                    true,
                    CreatePasswordHash(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    null,
                    false,
                    false,
                    null,
                    true,
                    0,
                    User.DefaultTenant,
                    true
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: ["UserId", "RoleId"],
                values: new object[,]
                {
                    { SuperAdminUserId, AdminRoleId },
                    { SuperAdminUserId, SuperAdminRoleId }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: ["UserId", "RoleId"],
                keyValues: [SuperAdminUserId, AdminRoleId]);

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: ["UserId", "RoleId"],
                keyValues: [SuperAdminUserId, SuperAdminRoleId]);

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: SuperAdminUserId);

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: AdminRoleId);

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: SuperAdminRoleId);

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Tenant",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsSuperUser",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Tenant",
                table: "AspNetUsers");
        }

        private static string CreatePasswordHash()
        {
            var hasher = new PasswordHasher<User>();
            var user = new User
            {
                Id = SuperAdminUserId,
                UserName = IdentitySeedOptions.SuperAdminEmail,
                Email = IdentitySeedOptions.SuperAdminEmail,
                FullName = IdentitySeedOptions.SuperAdminFullName,
                Tenant = User.DefaultTenant,
                IsSuperUser = true,
                EmailConfirmed = true
            };

            return hasher.HashPassword(user, IdentitySeedOptions.SuperAdminPassword);
        }
    }
}

using Microsoft.EntityFrameworkCore;

using PersonalFinance.Services.UserManagement.Application.Services;
using PersonalFinance.Services.UserManagement.Domain.Entities;

namespace PersonalFinance.Services.UserManagement.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(UserManagementDbContext context, IPasswordHasher passwordHasher)
        {
            // Update the Database if there are pending migrations
            if (context.Database.GetPendingMigrations().Any())
            {
                await context.Database.MigrateAsync();
            }

            // Seed Roles
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null)
            {
                adminRole = new Role("Admin", "Full access to all system data");
                await context.Roles.AddAsync(adminRole);
            }

            var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole == null)
            {
                userRole = new Role("User", "Standard user access to personal data");
                await context.Roles.AddAsync(userRole);
            }

            await context.SaveChangesAsync();

            // Seed Admin User
            var adminEmail = "admin@personalfinance.com";
            var adminUser = await context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Email.Value == adminEmail);

            if (adminUser == null)
            {
                var admin = new User(adminEmail, "admin", "System", "Administrator");
                admin.SetPasswordHash(passwordHasher.HashPassword("Admin@123"));
                admin.ConfirmEmail(); // Pre-confirm admin email

                admin.AddRole(adminRole);

                var profile = new UserProfile(admin.Id);

                await context.Users.AddAsync(admin);
                await context.UserProfiles.AddAsync(profile);
                await context.SaveChangesAsync();
            }
            else if (!adminUser.UserRoles.Any(ur => ur.RoleId == adminRole.Id))
            {
                // Ensure existing admin has the role
                adminUser.AddRole(adminRole);
                await context.SaveChangesAsync();
            }
        }
    }
}

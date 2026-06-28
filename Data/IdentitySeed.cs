using Microsoft.AspNetCore.Identity;

namespace car_rental_system.Data
{
    public static class IdentitySeed
    {
        public const string AdminRole = "Admin";
        public const string UserRole = "User";
        public const string DefaultAdminEmail = "admin@carrental.com";
        public const string DefaultAdminPassword = "Admin@123";

        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

            foreach (var roleName in new[] { AdminRole, UserRole })
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            var admin = await userManager.FindByEmailAsync(DefaultAdminEmail);
            if (admin == null)
            {
                admin = new IdentityUser
                {
                    UserName = DefaultAdminEmail,
                    Email = DefaultAdminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, DefaultAdminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, AdminRole);
            }
            else if (!await userManager.IsInRoleAsync(admin, AdminRole))
            {
                await userManager.AddToRoleAsync(admin, AdminRole);
            }
        }
    }
}

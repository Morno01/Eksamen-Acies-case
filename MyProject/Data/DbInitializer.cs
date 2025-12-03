using Microsoft.AspNetCore.Identity;
using MyProject.Models;

namespace MyProject.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Opret roller
            string[] roleNames = { "SuperUser", "NormalUser" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Opret SuperUser
            var superUserEmail = "admin@acies.dk";
            var superUser = await userManager.FindByEmailAsync(superUserEmail);

            if (superUser == null)
            {
                superUser = new ApplicationUser
                {
                    UserName = superUserEmail,
                    Email = superUserEmail,
                    FullName = "Administrator",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(superUser, "admin");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(superUser, "SuperUser");
                }
            }

            // Opret Normal User
            var normalUserEmail = "bruger@acies.dk";
            var normalUser = await userManager.FindByEmailAsync(normalUserEmail);

            if (normalUser == null)
            {
                normalUser = new ApplicationUser
                {
                    UserName = normalUserEmail,
                    Email = normalUserEmail,
                    FullName = "Normal Bruger",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(normalUser, "bruger");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(normalUser, "NormalUser");
                }
            }
        }
    }
}

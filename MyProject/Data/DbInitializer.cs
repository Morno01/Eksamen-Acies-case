using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyProject.Models;

namespace MyProject.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<PalleOptimeringContext>();

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

            // Seed test elementer hvis der ikke er nogen
            if (!await context.Elementer.AnyAsync())
            {
                var elementer = new List<Element>
                {
                    new Element
                    {
                        Reference = "DØR-001",
                        Type = "Dør",
                        Serie = "Premium",
                        Hoejde = 2100,
                        Bredde = 900,
                        Dybde = 100,
                        Vaegt = 45.5m,
                        ErSpecialelement = false,
                        ErGeometrielement = false,
                        RotationsRegel = "Ja"
                    },
                    new Element
                    {
                        Reference = "VIND-001",
                        Type = "Vindue",
                        Serie = "Premium",
                        Hoejde = 1200,
                        Bredde = 1200,
                        Dybde = 100,
                        Vaegt = 35.0m,
                        ErSpecialelement = false,
                        ErGeometrielement = false,
                        RotationsRegel = "Ja"
                    },
                    new Element
                    {
                        Reference = "DØR-002",
                        Type = "Dør",
                        Serie = "Standard",
                        Hoejde = 2000,
                        Bredde = 800,
                        Dybde = 100,
                        Vaegt = 40.0m,
                        ErSpecialelement = false,
                        ErGeometrielement = false,
                        RotationsRegel = "Ja"
                    },
                    new Element
                    {
                        Reference = "SPEC-001",
                        Type = "Special",
                        Serie = "Custom",
                        Hoejde = 2500,
                        Bredde = 1500,
                        Dybde = 150,
                        Vaegt = 85.0m,
                        ErSpecialelement = true,
                        ErGeometrielement = true,
                        RotationsRegel = "Nej"
                    }
                };

                await context.Elementer.AddRangeAsync(elementer);
                await context.SaveChangesAsync();
            }
        }
    }
}

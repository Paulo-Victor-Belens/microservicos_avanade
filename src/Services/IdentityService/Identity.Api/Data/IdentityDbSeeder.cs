using Identity.Api.Model;
using Microsoft.AspNetCore.Identity;
using IdGen;

namespace Identity.Api.Data
{
    public static class IdentityDbSeeder
    {
        public static async Task Seed(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<long>> roleManager, IdGenerator idGenerator, ILogger logger)
        {
            logger.LogInformation("Verificando se as Roles existem...");
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole<long>("Admin") { Id = idGenerator.CreateId() });
                logger.LogInformation("Role 'Admin' criada.");
            }

            if (!await roleManager.RoleExistsAsync("Customer"))
            {
                await roleManager.CreateAsync(new IdentityRole<long>("Customer") { Id = idGenerator.CreateId() });
                logger.LogInformation("Role 'Customer' criada.");
            }

            logger.LogInformation("Verificando se o usuário Admin existe...");
            var adminUser = await userManager.FindByEmailAsync("admin@ecommerce.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    Id = idGenerator.CreateId(),
                    UserName = "admin@ecommerce.com",
                    Email = "admin@ecommerce.com",
                    FullName = "Administrador do Sistema",
                    MemberSince = DateTime.UtcNow,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    logger.LogInformation("Usuário 'Admin' criado e associado à role Admin.");
                }
                else
                {
                    logger.LogError("Falha ao criar usuário Admin: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
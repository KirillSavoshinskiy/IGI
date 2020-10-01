using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Online_Auction.Models;

namespace Online_Auction.Helpers
{
    public class AdminRoleInitializer
    {
        public static async Task InitializeRolesAdmin(UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            string adminName = "admin";
            string adminEmail = "savosh28@gmail.com";
            string adminPassword = "Password123#@!";

            if (await roleManager.FindByIdAsync("admin") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("admin"));
            }
            if (await roleManager.FindByIdAsync("user") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("user"));
            }

            if (await userManager.FindByNameAsync(adminName) == null)
            {
                var admin = new User {UserName = adminName, Email = adminEmail, EmailConfirmed = true};
                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "admin");
                }
            }
        }
    }
}
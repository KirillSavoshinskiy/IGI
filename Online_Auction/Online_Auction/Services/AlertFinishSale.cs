using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Online_Auction.Data;
using Online_Auction.Models;

namespace Online_Auction.Services
{
    public class AlertFinishSale 
    {
        IServiceProvider _serviceProvider;
        public AlertFinishSale(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public async Task Alert( ) 
        {
            using (IServiceScope scope = _serviceProvider.CreateScope())
            await using (var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>())
            {
                var lots = await context.Lots
                    .Where(d => (d.FinishSale < DateTime.UtcNow.AddHours(3)) && !d.SentEmail)
                    .Include(u => u.User).ToListAsync();
                using (var userManager = scope.ServiceProvider.GetService<UserManager<User>>())
                {
                    var emailService = new EmailService(); 
                    foreach (var lot in lots.Where(d => (d.FinishSale < DateTime.UtcNow.AddHours(3)) && !d.SentEmail))
                    {
                        lot.SentEmail = true;
                        context.Lots.Update(lot);
                        await context.SaveChangesAsync();
                        if (lot.UserPriceId != lot.UserId && !string.IsNullOrEmpty(lot.UserPriceId))
                        {
                            var user = await userManager.FindByIdAsync(lot.UserPriceId);
                            await emailService.SendEmailAsync(user.Email, "Вы победили в торгах",
                                $"Вы выкупили лот: {lot.Name}");
                            await emailService.SendEmailAsync(lot.User.Email, "Информация по лоту",
                                $"Торги на ваш лот {lot.Name} закончились");
                        }
                        else
                        {
                            await emailService.SendEmailAsync(lot.User.Email, "Информация по лоту",
                                $"Ваш лот {lot.Name} никто не выкупил");
                        }
                    }
                }
            }
        }
    }
}
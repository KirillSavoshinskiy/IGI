﻿using System; 
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IGI.Data;
using IGI.Models;

namespace IGI.Services
{
    public class AlertFinishSale 
    {
        private IServiceProvider _serviceProvider;
        private IConfiguration Configuration;
        public AlertFinishSale(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            Configuration = configuration;
        }
        public async Task Alert() 
        {
            using (var scope = _serviceProvider.CreateScope())
            await using (var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>())
            {
                var all =  await context.Lots.Include(u => u.User).ToListAsync();
                var lots = all.Where(d => (d.FinishSale < DateTime.Now.AddHours(d.Hours)) && !d.SentEmail).ToList();
                using (var userManager = scope.ServiceProvider.GetService<UserManager<User>>())
                {
                    var emailService = new EmailService(Configuration); 
                    foreach (var lot in lots)
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
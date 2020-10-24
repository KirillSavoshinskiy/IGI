using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Online_Auction.Data;
using Online_Auction.Models;

namespace Online_Auction.Services
{
    public class AlertFinishSale: IAlertFinishSale
    {
        public async Task Alert(List<Lot> lots, ApplicationContext context, IEmailService emailService
        , UserManager<User> userManager)//////////////
        {
            foreach (var lot in lots.Where(d => (d.FinishSale < DateTime.UtcNow.AddHours(3)) && !d.SentEmail))
            {
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

                lot.SentEmail = true;
                await context.SaveChangesAsync();
            }
        }
    }
}
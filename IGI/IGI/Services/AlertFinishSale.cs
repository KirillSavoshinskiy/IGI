using System;
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
        private ApplicationContext _context;
        private IConfiguration Configuration;
        private UserManager<User> _userManager;

        public AlertFinishSale(ApplicationContext context, IConfiguration configuration, UserManager<User> userManager)
        {
            _context = context;
            Configuration = configuration;
            _userManager = userManager;
        }

        public async Task Alert()
        {
            var all = await _context.Lots.Include(u => u.User).ToListAsync();
            var lots = all.Where(d => (d.FinishSale < DateTime.Now.AddHours(d.Hours)) && !d.SentEmail).ToList();

            var emailService = new EmailService(Configuration);
            foreach (var lot in lots)
            {
                lot.SentEmail = true;
                _context.Lots.Update(lot);
                await _context.SaveChangesAsync();
                if (lot.UserPriceId != lot.UserId && !string.IsNullOrEmpty(lot.UserPriceId))
                {
                    var user = await _userManager.FindByIdAsync(lot.UserPriceId);
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
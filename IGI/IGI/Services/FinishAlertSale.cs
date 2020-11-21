using System;
using System.Linq;
using System.Threading.Tasks;
using IGI.Data;
using IGI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace IGI.Services
{
    public class FinishAlertSale
    {
        private ApplicationContext _context;
        private IConfiguration Configuration;
        private UserManager<User> _userManager;

        public FinishAlertSale(ApplicationContext context, IConfiguration configuration, UserManager<User> userManager)
        {
            _context = context;
            Configuration = configuration;
            _userManager = userManager;
        }

        public async Task Alert(int lotId)
        {
            var lot = await _context.Lots.Where(i => i.Id == lotId)
                .Include(u => u.User).FirstAsync();
            var emailService = new EmailService(Configuration);
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
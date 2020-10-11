using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Online_Auction.Data;
using Online_Auction.Models;

namespace Online_Auction.Services
{
    public interface IAlertFinishSale
    {
        public Task Alert(List<Lot> lots, ApplicationContext context, IEmailService emailService
        , UserManager<User> userManager);
    }
}
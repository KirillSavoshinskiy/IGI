﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using IGI.Data;
using IGI.Models;

namespace Online_Auction.Hubs
{
    public class RateHub : Hub
    {
        private ApplicationContext _context;
        private UserManager<User> _userManager;

        public RateHub(ApplicationContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task SendRate(string userName, string rate, string lotId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, lotId);
            bool flag = false;
            var lot = await _context.Lots.Where(i => i.Id == Int32.Parse(lotId))
                .Include(u => u.User).FirstAsync();
            var user = await _userManager.FindByNameAsync(userName);
            if (lot.User.UserName == userName)
            {
                await Clients.Caller.SendAsync("AlertOwner", "Нельзя ставить на свой лот");
                flag = true;
            }

            if (rate == "")
            {
                await Clients.Caller.SendAsync("Alert", "Введите ставку");
            }

            if (lot.Price >= Decimal.Parse(rate))
            {
                await Clients.Caller.SendAsync("Alert", "Ставка слишком мала");
            }
            else if (!flag)
            {
                await Clients.Group(lotId).SendAsync("ReceiveRate", userName, rate);
                lot.Price = Decimal.Parse(rate);
                lot.UserPriceId = user.Id;
                _context.Lots.Update(lot);
                await _context.SaveChangesAsync();
            }
        }
    }
}
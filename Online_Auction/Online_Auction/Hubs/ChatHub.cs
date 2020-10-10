using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Online_Auction.Data;
using Online_Auction.Models;

namespace Online_Auction.Hubs
{
    public class ChatHub: Hub
    { 
        private ApplicationContext _context;
        private UserManager<User> _userManager; 
        public ChatHub(ApplicationContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task SendMessage(string user, string message, string lotId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, lotId);
            await Clients.Group(lotId).SendAsync("ReceiveMessage", user, message);
            var comment = new Comment()
            {
                CommentText = message,
                User = await _userManager.FindByNameAsync(Context.User.Identity.Name),
                LotId = Int32.Parse(lotId)
            };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
        }
    }
}
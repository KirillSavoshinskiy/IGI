using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Online_Auction.Data;
using Online_Auction.Models;

namespace Online_Auction.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private UserManager<User> _userManager; 
        private ApplicationContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationContext context, UserManager<User> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        { 
            return View(_context.Lots.Include(img => img.Images)
                .Include(u => u.User).Include(c => c.Category));
        }

        [HttpGet]
        public IActionResult ProfileLot(int id)
        {
            return View(_context.Lots.Where(i => i.Id == id).Include(img => img.Images)
                .Include(u => u.User).Include(c => c.Category).First());
        }

        [HttpPost]
        public async Task<IActionResult> IncreasePrice(int id, decimal price)
        {
            var lot = await _context.Lots.FindAsync(id);
            var owner = await _userManager.FindByIdAsync(lot.UserId);
            if (owner.UserName == User.Identity.Name)
            {
                return Content("Вы не можете делать ставки на свой лот");
            }
            var user = _userManager.Users.First(i => i.UserName == User.Identity.Name);
             
            if (price > lot.Price)
            {
                lot.Price = price;
                lot.UserPriceId = user.Id;
                _context.Lots.Update(lot);
                await _context.SaveChangesAsync();
                return RedirectToAction("ProfileLot", "Home", new {id = lot.Id});
            }
            return Content("Введённая ставка ниже прежней");
        }        
        
        
        
        
        
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}
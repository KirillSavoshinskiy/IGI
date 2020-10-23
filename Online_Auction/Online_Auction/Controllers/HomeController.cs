using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Online_Auction.Data;
using Online_Auction.Hubs;
using Online_Auction.Models;
using Online_Auction.Services;
using Online_Auction.ViewModels;

namespace Online_Auction.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private UserManager<User> _userManager; 
        private ApplicationContext _context; 
        private IEmailService _emailService;
        private IAlertFinishSale _alertFinishSale;

        public HomeController(ILogger<HomeController> logger, ApplicationContext context, UserManager<User> userManager
        , IEmailService emailService, IAlertFinishSale alertFinishSale)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _alertFinishSale = alertFinishSale;
        } 

        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            var countLots = 10;
            var lots = _context.Lots.Include(img => img.Images)
                .Include(u => u.User).Include(c => c.Category)
                .ToList(); 
            await _alertFinishSale.Alert(lots, _context, _emailService, _userManager);
            PageViewModel pageViewModel = new PageViewModel(lots.Count, pageIndex, countLots);
            var items =  lots.Skip((pageIndex - 1) * countLots).Take(countLots).ToList();
            LotViewModel viewModel = new LotViewModel
            {
                PageViewModel = pageViewModel,
                Lots = items
            };
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult ProfileLot(int id)
        {
            ProfileLotViewModel viewModel = new ProfileLotViewModel()
            {
                Lot = _context.Lots.Where(i => i.Id == id).Include(img => img.Images)
                    .Include(u => u.User).Include(c => c.Category).First(),
                Comments = _context.Comments.Where(i => i.LotId == id)
                    .Include(u => u.User).ToList()
            };
            return View(viewModel);
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
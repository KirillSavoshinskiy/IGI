using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq; 
using System.Threading.Tasks;
using Hangfire; 
using Microsoft.AspNetCore.Mvc; 
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Online_Auction.Data; 
using Online_Auction.Models;
using Online_Auction.Services;
using Online_Auction.ViewModels;

namespace Online_Auction.Controllers
{
    public class HomeController : Controller
    {  
        private ApplicationContext _context;  

        public HomeController(ApplicationContext context)
        { 
            _context = context; 
        } 

        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            var countLots = 10;
            var lots = await _context.Lots.Include(img => img.Images)
                .Include(u => u.User).Include(c => c.Category)
                .ToListAsync();
             
            
            RecurringJob.AddOrUpdate<AlertFinishSale>(x => x.Alert(), Cron.Minutely); 
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
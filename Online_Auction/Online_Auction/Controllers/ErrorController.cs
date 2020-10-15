using Microsoft.AspNetCore.Mvc;

namespace Online_Auction.Controllers
{
    public class ErrorController: Controller
    { 
        public IActionResult Index(int? statusCode = null)
        {
            ViewData["Error"] = statusCode;  
            return View();
        }
    }
}
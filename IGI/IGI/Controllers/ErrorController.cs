﻿using Microsoft.AspNetCore.Mvc;

namespace IGI.Controllers
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
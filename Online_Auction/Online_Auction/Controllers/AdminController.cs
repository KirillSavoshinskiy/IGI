using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Online_Auction.Data;
using Online_Auction.Models;
using Online_Auction.Services;
using Online_Auction.ViewModels;

namespace Online_Auction.Controllers
{
    [Authorize(Roles = "admin")] 
    public class AdminController: Controller
    {
        private UserManager<User> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ApplicationContext _context;
        IWebHostEnvironment _appEnvironment;
        private IEmailService _emailService;
        private ISaveImage _saveImage;

        public AdminController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ApplicationContext context
        , IWebHostEnvironment appEnvironment, IEmailService emailService, ISaveImage saveImage)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _appEnvironment = appEnvironment;
            _emailService = emailService;
            _saveImage = saveImage;
        }
        
        public IActionResult IndexPanel() => View();

        public IActionResult IndexLots() 
        { 
            return View( _context.Lots.Include(i => i.User)
                .Include(img => img.Images)
                .ToList());
        }

        [HttpGet]
        public IActionResult CreateLot()
        {
            return RedirectToAction("CreateLot", "Account");
        }

         
        
        [HttpGet]
        public IActionResult ProfileLot(int id)
        {
            return RedirectToAction("ProfileLot", "Home", new {id = id});
        }
        
        [HttpPost]
        public async Task<IActionResult> DeleteLot(int id)
        {
            var lot = await _context.Lots.FindAsync(id);
            if (lot != null)
            {
                _context.Lots.Remove(lot);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("IndexLots");
        }
        
        /// <summary>
        /// categories functionality
        /// </summary> 
        public IActionResult IndexCategories() => View(_context.Categories.ToList());

        [HttpGet]
        public IActionResult CreateCategory() => View();

        [HttpPost]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("IndexCategories");
        }
        
        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id) //later maybe add else block
        {
            var category = await _context.Categories.FindAsync(id); 
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("IndexCategories");
        }
    
        /// <summary>
        /// user functionality
        /// </summary> 
        public IActionResult IndexUsers()
        {
            return View(_userManager.Users.ToList());
        }
        [HttpGet]
        public IActionResult Create() => RedirectToAction("Register", "Account");
        
        [HttpGet]
        public IActionResult ProfileUser(string name) => 
            RedirectToAction("Profile", "Account", new {name = name});
 

        [HttpGet]
        public async Task<IActionResult> SendMail(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            MailViewModel viewModel = new MailViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SendMail(MailViewModel viewModel)
        { 
               User user = await _userManager.FindByIdAsync(viewModel.Id);
               if (user == null)
               {
                   return NotFound();
               }  
               await _emailService.SendEmailAsync(user.Email, viewModel.Subject, viewModel.Message);
               return RedirectToAction("IndexUsers");
        }
        
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.ToList();
            EditUserViewModel model = new EditUserViewModel
            {
                Id = user.Id, 
                UserName = user.UserName, 
                Email = user.Email,
                UserRoles = userRoles,
                AllRoles = allRoles
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel viewModel, List<string> roles)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByIdAsync(viewModel.Id);
                if (user != null)
                {
                    var userRoles = await _userManager.GetRolesAsync(user); 
                    var addedRoles = roles.Except(userRoles);
                    var removedRoles = userRoles.Except(roles);
                    
                    user.UserName = viewModel.UserName;
                    user.Email = viewModel.Email;

                    var result = await _userManager.UpdateAsync(user);
                    await _userManager.AddToRolesAsync(user, addedRoles);
                    await _userManager.RemoveFromRolesAsync(user, removedRoles);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("IndexUsers");
                    }

                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError("", item.Description);
                    }
                    
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id) //later maybe add else block
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
                
            }
            return RedirectToAction("IndexUsers");
        }
    }
}
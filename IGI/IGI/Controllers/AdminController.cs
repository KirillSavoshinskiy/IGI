﻿using System;
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
using IGI.Data;
using IGI.Models;
using IGI.Services;
using IGI.ViewModels;

namespace IGI.Controllers
{
    [Authorize(Roles = "admin")] 
    public class AdminController: Controller
    {
        private UserManager<User> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ApplicationContext _context; 
        private IEmailService _emailService; 

        public AdminController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ApplicationContext context
        , IEmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context; 
            _emailService = emailService; 
        }
        
        public IActionResult IndexPanel() => View();

        public IActionResult IndexLots() 
        { 
            return View( _context.Lots.Include(i => i.User)
                .Include(img => img.Images)
                .ToList());
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
                await _context.Categories.AddAsync(category);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("IndexCategories");
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(Category category)
        {
            var cat = await _context.Categories.FindAsync(category.Id);
            cat.Name = category.Name;
            _context.Categories.Update(cat);
            await _context.SaveChangesAsync();
            return RedirectToAction("IndexCategories");
        }
        
        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id) //later maybe add else block
        {
            var category = _context.Categories.Where(i => i.Id == id)
                .Include(l => l.Lots).First();
            if (category != null)
            {
                foreach (var lot in category.Lots)
                {
                    lot.CategoryId = null;
                }
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("IndexCategories");
        }
    
        /// <summary>
        /// user functionality
        /// </summary> 
        public IActionResult IndexUsers() => View(_userManager.Users.ToList());

        [HttpGet]
        public async Task<IActionResult> SendMail(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var viewModel = new MailViewModel
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
               var user = await _userManager.FindByIdAsync(viewModel.Id);
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
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.ToListAsync();
            var model = new EditUserViewModel
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
                var user = await _userManager.FindByIdAsync(viewModel.Id);
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
        public async Task<IActionResult> Delete(string id)  
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user.UserName == "admin")
            {
                return Content("Вы не можете удалить админа");
            }
            
            if (user != null)
            {
                var lots = await _context.Lots.Where(u => u.UserId == user.Id).ToListAsync();
                _context.Lots.RemoveRange(lots);
                await _context.SaveChangesAsync();
                await _userManager.DeleteAsync(user);
                
            }
            return RedirectToAction("IndexUsers");
        }
    }
}
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

        public AdminController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ApplicationContext context
        , IWebHostEnvironment appEnvironment)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _appEnvironment = appEnvironment;
        }
        
        public IActionResult IndexPanel() => View();

        public IActionResult IndexLots()//////////////////////////////////////
        {
            var lots = _context.Lots.Include(i => i.User)
                .ToList();
            var images = _context.Images.ToList();
            var pairs = new Dictionary<Lot, Img>();
            foreach (var lot in lots)
            {
                   pairs.Add(lot, images.First(i => i.LotId == lot.Id));
            }
            return View(pairs);
        }

        [HttpGet]
        public IActionResult CreateLot()
        {
            ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateLot(CreateLotViewModel viewModel)
        { 
            var users = _userManager.Users;
            List<string> imgs = new List<string>();
            foreach (var user in users) 
            { 
                if (User.Identity.Name == user.UserName) 
                { 
                    viewModel.User = user; 
                }
            }
            
            if (ModelState.IsValid)
            {
                Lot lot = new Lot
                {
                    Name = viewModel.Name,
                    Description = viewModel.Description,
                    Price = viewModel.Price,
                    StartSale = viewModel.StartSale,
                    FinishSale = viewModel.FinishSale,
                    User = viewModel.User,
                    CategoryId = viewModel.CategoryId 
                };
                _context.Lots.Add(lot);
                await _context.SaveChangesAsync(); 
                foreach (var image in viewModel.Images)
                { 
                    string path = "/Files/" + image.FileName; 
                    await using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }
                    Img img = new Img
                    {
                        ImgPath = path,
                        Name = image.Name,
                        LotId = lot.Id
                    };
                    _context.Images.Add(img);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction("IndexLots");
            }
            ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "Id", "Name");
            return View(viewModel);
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
        public IActionResult Create() => View();
        
        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                User user = new User {UserName = viewModel.UserName, Email = viewModel.Email}; 
                var result = await _userManager.CreateAsync(user, viewModel.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "user");
                    var confToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confUrl = Url.Action(
                        "ConfirmEmail",
                        "Account",
                        new {userId = user.Id, token = confToken},
                        protocol: HttpContext.Request.Scheme);
                    EmailService emailService = new EmailService();
                    await emailService.SendEmailAsync(viewModel.Email, "Подтверждение регистрации",
                        $"Подтвердите регистрацию, перейдя по ссылке: <a href='{confUrl}'>link</a>");
                    return Content("Для завершения регистрации проверьте электронную почту и перейдите по ссылке, указанной в письме");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(viewModel);
        }
        
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return View("Error");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null )
            {
                return View("Error");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            { 
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Profile(string id)
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
               EmailService emailService = new EmailService(); 
               await emailService.SendEmailAsync(user.Email, viewModel.Subject, viewModel.Message);
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
                    var allRoles = _roleManager.Roles.ToList();
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
                    else
                    {
                        foreach (var item in result.Errors)
                        {
                            ModelState.AddModelError("", item.Description);
                        }
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
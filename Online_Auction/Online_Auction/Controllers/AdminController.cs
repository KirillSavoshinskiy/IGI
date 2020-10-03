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

        public AdminController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ApplicationContext context
        , IWebHostEnvironment appEnvironment)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _appEnvironment = appEnvironment;
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
            ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateLot(CreateLotViewModel viewModel)
        {   
            viewModel.User = _userManager.Users.First(i => i.UserName == User.Identity.Name); 
           
            if (viewModel.StartSale < DateTime.Now)
            {
                ModelState.AddModelError("", "Старт торгов не может быть раньше чем сейчас" );
            }

            if (viewModel.StartSale > viewModel.FinishSale)
            {
                ModelState.AddModelError("", "Конец торгов не может быть раньше чем старт" );
            }
            
            if (ModelState.IsValid)
            {
                var lot = new Lot
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
                    var path = "/Files/" + image.FileName; 
                    await using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }
                    var img = new Img
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

        [HttpGet]
        public IActionResult EditLot(int id)
        {
            var lot = _context.Lots.Include(i => i.User)
                .Include(img => img.Images) 
                .First(i => i.Id == id);
            if (lot.StartSale < DateTime.Now)
            {
                return Content("Вы не можете изменять лот, так как торги начались");
            }

            if (lot.FinishSale < DateTime.Now)
            {
                return Content("Вы не можете изменять лот, так как торги закончились");
            }
            EditLotViewModel viewModel = new EditLotViewModel
            {
                Id = lot.Id,
                Name = lot.Name,
                Description = lot.Description,
                Price = lot.Price,
                StartSale = lot.StartSale,
                FinishSale = lot.FinishSale,
                CategoryId = lot.CategoryId
            };
            ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "Id", "Name");
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditLot(EditLotViewModel viewModel)
        {
            if (viewModel.StartSale < DateTime.Now)
            {
                ModelState.AddModelError("", "Старт торгов не может быть раньше чем сейчас" );
            }
            if (viewModel.StartSale > viewModel.FinishSale)
            {
                ModelState.AddModelError("", "Конец торгов не может быть раньше чем старт" );
            }
            if (ModelState.IsValid)
            {
                var lot = await _context.Lots.FindAsync(viewModel.Id);
                lot.Name = viewModel.Name;
                lot.Description = viewModel.Description;
                lot.Price = viewModel.Price;
                lot.StartSale = viewModel.StartSale;
                lot.FinishSale = viewModel.FinishSale;
                lot.CategoryId = viewModel.CategoryId;
                _context.Lots.Update(lot);
                await _context.SaveChangesAsync();
                return RedirectToAction("IndexLots", "Admin");
            }
            ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "Id", "Name");
            return View(viewModel);
        }
        
        [HttpGet]
        public async Task<IActionResult> ProfileLot(int id)
        {
            var lot = await _context.Lots.Where(l => l.Id == id).Include(i => i.User)
                .Include(img => img.Images).Include(c => c.Category).FirstAsync();
            return View(lot);
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
                return RedirectToAction("ProfileLot", "Admin", new {id = lot.Id});
            }
            return Content("Введённая ставка ниже прежней");
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
        public async Task<IActionResult> ProfileUser(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            var userRoles = await _userManager.GetRolesAsync(user);
            var userLots = await _context.Lots.Where(i => i.UserId == user.Id)
                .Include(c => c.Category).Include(i => i.Images).ToListAsync(); 

            if (user == null)
            {
                return NotFound();
            }
            ProfileViewModel viewModel = new ProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                UserRoles = userRoles,
                Lots = userLots
            };
            
            return View(viewModel);
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
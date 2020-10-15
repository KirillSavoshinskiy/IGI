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
     
    public class AccountController: Controller
    {
        private UserManager<User> _userManager;
        private SignInManager<User> _signInManager; 
        private ApplicationContext _context;
        IWebHostEnvironment _appEnvironment;
        private IEmailService _emailService;
        private ISaveImage _saveImage;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, 
            IWebHostEnvironment appEnvironment, ApplicationContext context, IEmailService emailService,
            ISaveImage saveImage)
        {
            _userManager = userManager;
            _signInManager = signInManager; 
            _context = context;
            _appEnvironment = appEnvironment;
            _emailService = emailService;
            _saveImage = saveImage;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Register(CreateUserViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var user = new User {UserName = viewModel.UserName, Email = viewModel.Email};
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
                    await _emailService.SendEmailAsync(viewModel.Email, "Подтверждение регистрации",
                        $"Подтвердите регистрацию, перейдя по ссылке: <a href='{confUrl}'>link</a>");
                    return Content(
                        "Для завершения регистрации проверьте электронную почту и перейдите по ссылке, указанной в письме");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Profile(string name)
        {
            if (User.Identity.Name != name && !User.IsInRole("admin"))
            {
                return Content("Вы пытаетесь войти в чужой профиль");
            }
            var user = await _userManager.FindByNameAsync(name);
            var userRoles = await _userManager.GetRolesAsync(user);
            var userLots = await _context.Lots.Where(i => i.UserId == user.Id)
                .Include(c => c.Category).Include(i => i.Images).ToListAsync(); 
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new ProfileViewModel
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
        public async Task<IActionResult> EditProfile(string id)
        {
            var user = await _userManager.FindByNameAsync(id);
            if (user == null)
            {
                return NotFound();
            }  
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(User usr)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(usr.Id);
                if (user != null)
                {
                    user.UserName = usr.UserName; 
                    if (user.Email != usr.UserName)
                    {
                        user.EmailConfirmed = false;
                    }
                    user.Email = usr.Email;
                    
                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        if (!user.EmailConfirmed)
                        {
                            var confToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                            var confUrl = Url.Action(
                                "ConfirmEmail",
                                "Account",
                                new {userId = user.Id, token = confToken},
                                protocol: HttpContext.Request.Scheme);
                            await _emailService.SendEmailAsync(usr.Email, "Подтверждение почты",
                                $"Подтвердите почту, перейдя по ссылке: <a href='{confUrl}'>link</a>");
                            return Content(
                                "Для завершения изменения профиля проверьте электронную почту и перейдите по ссылке, указанной в письме");
                        }
                    }

                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError("", item.Description);
                    }
                }
            }
            return View(usr);
        }
        
        
        
        [HttpGet]
        [Authorize]
        public IActionResult CreateLot()
        {
            ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateLot(CreateLotViewModel viewModel)
        { 
            var users = _userManager.Users;
            var imgs = new List<string>();
            foreach (var user in users) 
            { 
                if (User.Identity.Name == user.UserName) 
                { 
                    viewModel.User = user; 
                }
            }

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
                await _saveImage.SaveImg(viewModel.Images, _context, _appEnvironment, lot); 
                return RedirectToAction("Index", "Home"); 
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
            if (User.Identity.Name != lot.User.UserName && !User.IsInRole("admin"))
            {
                return Content("Вы пытаетесь войти в чужой профиль");
            }
            if ((lot.StartSale < DateTime.Now) && (lot.FinishSale > DateTime.Now))
            {
                return Content("Вы не можете изменять лот, так как торги начались");
            }

            if (lot.FinishSale < DateTime.Now)
            {
                return Content("Вы не можете изменять лот, так как торги закончились");
            }
            var viewModel = new EditLotViewModel
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
                var lot = _context.Lots.Where(i => i.Id ==viewModel.Id)
                    .Include(u => u.User).First();
                if (User.Identity.Name != lot.User.UserName && !User.IsInRole("admin"))
                {
                    return Content("Вы пытаетесь войти в чужой профиль");
                }
                lot.Name = viewModel.Name;
                lot.Description = viewModel.Description;
                lot.Price = viewModel.Price;
                lot.StartSale = viewModel.StartSale;
                lot.FinishSale = viewModel.FinishSale;
                lot.CategoryId = viewModel.CategoryId;
                lot.SentEmail = false;
                _context.Images.RemoveRange(_context.Images.Where(i => i.LotId == lot.Id));
                await _saveImage.SaveImg(viewModel.Images, _context, _appEnvironment, lot);  
                _context.Lots.Update(lot);
                await _context.SaveChangesAsync();
                return RedirectToAction("Profile", "Account", new{ name = lot.User.UserName});
            }
            ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "Id", "Name");
            return View(viewModel);
        }
        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteLot(int id)
        {
            var lot = _context.Lots.Where(i => i.Id == id)
                .Include(u => u.User).First();
            if (User.Identity.Name != lot.User.UserName && !User.IsInRole("admin"))
            {
                return Content("Вы пытаетесь войти в чужой профиль");
            } 
            _context.Lots.Remove(lot);
            await _context.SaveChangesAsync();
             

            return RedirectToAction("Profile", new{ name = lot.User.UserName});
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
                await _signInManager.SignInAsync(user, false);
                return RedirectToAction("Index", "Home");
            } 
            return View("Error");
             
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var result =
                    await _signInManager.PasswordSignInAsync(viewModel.UserName, viewModel.Password, viewModel.RememberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                ModelState.AddModelError("", "Неправильный логин или пароль");
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
     
}
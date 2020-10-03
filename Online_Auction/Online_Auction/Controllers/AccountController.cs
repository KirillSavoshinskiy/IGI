using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, 
            IWebHostEnvironment appEnvironment, ApplicationContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager; 
            _context = context;
            _appEnvironment = appEnvironment;
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
        public async Task<IActionResult> Profile(string name)
        {
            User user = await _userManager.FindByNameAsync(name);
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
                return RedirectToAction("Index", "Home");////добавить потом переадресацию на лот
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
                return RedirectToAction("Profile", "Account");
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

            return RedirectToAction("Profile");
        }
        
        [HttpGet]
        public IActionResult ProfileLot(int id)
        {
            return View(_context.Lots.Where(i => i.Id == id).Include(img => img.Images)
                .Include(u => u.User).Include(c => c.Category).First());
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
            else
            {
                return View("Error");
            }
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
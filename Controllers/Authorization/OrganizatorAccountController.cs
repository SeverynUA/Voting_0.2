using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Voting_0._2.Data.Entities.Users;
using Voting_0._2.Models.DTOs.Account;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace Voting_0._2.Controllers.Authorization
{
    [Route("OrganizatorAccount")]
    public class OrganizatorAccountController : Controller
    {
        private readonly UserManager<Account> _userManager;
        private readonly SignInManager<Account> _signInManager;

        private readonly ILogger<OrganizatorAccountController> _logger;

        public OrganizatorAccountController(UserManager<Account> userManager, SignInManager<Account> signInManager , ILogger<OrganizatorAccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Користувача не знайдено.");
                return NotFound("Користувача не знайдено.");
            }

            if (!User.IsInRole("Organizator"))
            {
                _logger.LogWarning("Користувач не має ролі Організатора.");
                return Forbid();
            }

            _logger.LogInformation("Користувач успішно увійшов з роллю Організатора.");

            return View(user);
        }


        // Сторінка реєстрації організатора
        [HttpGet("register-organizator")]
        public IActionResult RegisterOrganizator()
        {
            return View();
        }

        [HttpPost("register-organizator")]
        public async Task<IActionResult> RegisterOrganizator(AccountRegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var organizator = new Account { UserName = model.Email, Email = model.Email, FullName = model.FullName };
                var result = await _userManager.CreateAsync(organizator, model.Password);

                if (result.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(organizator, Roles.Organizator);
                    if (roleResult.Succeeded)
                    {
                        // Після успішної реєстрації перенаправляємо на сторінку входу
                        return RedirectToAction("Index", "OrganizatorAccount");
                    }
                    else
                    {
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }

        // Сторінка входу для організатора
        [HttpGet("login-organizator")]
        public IActionResult LoginOrganizator()
        {
            return View();
        }

        [HttpPost("login-organizator")]
        public async Task<IActionResult> LoginOrganizator(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    // Після успішного входу перенаправляємо на головну сторінку організатора
                    return RedirectToAction("Index", "OrganizatorAccount");
                }
                ModelState.AddModelError(string.Empty, "Невірний логін або пароль");
            }
            return View(model);
        }

        // Сторінка редагування облікового запису організатора
        [Authorize(Roles = Roles.Organizator)]
        [HttpGet("edit-account-organizator")]
        public IActionResult EditAccountOrganizator()
        {
            return View();
        }

        [Authorize(Roles = Roles.Organizator)]
        [HttpPost("edit-account-organizator")]
        public async Task<IActionResult> EditAccountOrganizator(EditAccountModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var organizator = await _userManager.FindByEmailAsync(model.Email);
            if (organizator == null)
            {
                ModelState.AddModelError(string.Empty, "Користувача не знайдено.");
                return View(model);
            }

            organizator.FullName = model.FullName;
            var result = await _userManager.UpdateAsync(organizator);

            if (result.Succeeded && !string.IsNullOrEmpty(model.NewPassword))
            {
                var passwordResult = await _userManager.ChangePasswordAsync(organizator, model.CurrentPassword, model.NewPassword);
                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return RedirectToAction("LoginOrganizator", "OrganizatorAccount");
        }

        // Сторінка зміни пароля організатора
        [Authorize(Roles = Roles.Organizator)]
        [HttpGet("change-password-organizator")]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordModel());
        }

        [Authorize(Roles = Roles.Organizator)]
        [HttpPost("change-password-organizator")]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("LoginOrganizator", "OrganizatorAccount");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "OrganizatorAccount");
            }

            // Якщо зміна пароля не вдалася, виводимо помилки
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // Логаут
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}

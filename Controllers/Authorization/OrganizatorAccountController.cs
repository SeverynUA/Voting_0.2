using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Voting_0._2.Data.Entities.Users;
using Voting_0._2.Models.DTOs.Account.Organizator;
using Voting_0._2.Models.DTOs.Account;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace Voting_0._2.Controllers.Authorization
{
    public class OrganizatorAccountController : Controller
    {
        private readonly UserManager<Account> _userManager;
        private readonly SignInManager<Account> _signInManager;

        public OrganizatorAccountController(UserManager<Account> userManager, SignInManager<Account> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [AllowAnonymous]
        public IActionResult AccessDenied(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound("Користувача не знайдено.");
            }
            return View(user);
        }

        [HttpGet("register-organizator")]
        public IActionResult RegisterOrganizator()
        {
            return View();
        }

        [HttpPost("register-organizator")]
        public async Task<IActionResult> RegisterOrganizator(OrganizatorRegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var organizator = new Account { UserName = model.Email, Email = model.Email, FullName = model.FullName };
                var result = await _userManager.CreateAsync(organizator, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(organizator, Roles.Organizator);
                    return RedirectToAction("Index", "OrganizatorAccount");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

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
                // Прапорець RememberMe передається у PasswordSignInAsync
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {

                    return RedirectToAction("Index", "OrganizatorAccount");
                }
                ModelState.AddModelError(string.Empty, "Невірний логін або пароль");
            }
            return View(model);
        }

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

            return RedirectToAction("LoginOrganizator" , "OrganizatorAccount");
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }

}

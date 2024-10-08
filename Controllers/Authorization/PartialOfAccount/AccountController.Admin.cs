﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Voting_0._2.Data.Entities.Users;
using Voting_0._2.Models.DTOs.Account;

namespace Voting_0._2.Controllers.Authorization
{
    // AccountController.Admin.cs
    public partial class AccountController : Controller
    {
        [Authorize(Roles = Roles.Admin)]
        [HttpGet("Admin/Index")]
        public async Task<IActionResult> AdminIndex()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Користувача не знайдено.");
            }
            return View(user);
        }

        [HttpGet("register-admin")]
        public IActionResult RegisterAdmin()
        {
            return View();
        }

        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin(AccountRegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var admin = new Account { UserName = model.Email, Email = model.Email, FullName = model.FullName };
                var result = await _userManager.CreateAsync(admin, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(admin, Roles.Admin);
                    return RedirectToAction("Index");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet("login-admin")]
        public IActionResult LoginAdmin()
        {
            return View();
        }

        [HttpPost("login-admin")]
        public async Task<IActionResult> LoginAdmin(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                // Прапорець RememberMe визначає, чи буде створено постійне кукі
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError(string.Empty, "Невірний логін або пароль");
            }
            return View(model);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpGet("edit-account-admin")]
        public async Task<IActionResult> EditAccountAdmin()
        {
            var admin = await _userManager.GetUserAsync(User);
            if (admin == null)
            {
                return RedirectToAction("Index");
            }

            var fullNameClaim = (await _userManager.GetClaimsAsync(admin)).FirstOrDefault(c => c.Type == "FullName");

            var model = new EditAccountModel
            {
                Email = admin.Email,
                FullName = fullNameClaim?.Value
            };

            return View(model);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPost("edit-account-admin")]
        public async Task<IActionResult> EditAccountAdmin(EditAccountModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var admin = await _userManager.FindByEmailAsync(model.Email);
            if (admin == null)
            {
                ModelState.AddModelError(string.Empty, "Користувача не знайдено.");
                return View(model);
            }

            // Оновлення або додавання нового Claims для FullName
            var claims = await _userManager.GetClaimsAsync(admin);
            var fullNameClaim = claims.FirstOrDefault(c => c.Type == "FullName");
            if (fullNameClaim != null)
            {
                await _userManager.RemoveClaimAsync(admin, fullNameClaim);
            }
            await _userManager.AddClaimAsync(admin, new Claim("FullName", model.FullName));

            var result = await _userManager.UpdateAsync(admin);

            if (result.Succeeded && !string.IsNullOrEmpty(model.NewPassword))
            {
                var passwordResult = await _userManager.ChangePasswordAsync(admin, model.CurrentPassword, model.NewPassword);
                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        return View(model); // Повертаємо модель у разі помилки пароля
                    }
                }
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpGet("change-password-admin")]
        public IActionResult ChangePasswordAdmin()
        {
            return View(new ChangePasswordModel());
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPost("change-password-admin")]
        public async Task<IActionResult> ChangePasswordAdmin(ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "AdminAccount");
            }

            // Якщо зміна пароля не вдалася, виводимо помилки
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

    }
}

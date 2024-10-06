using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Voting_0._2.Data.Entities;
using Voting_0._2.Models.DTOs.Account;

namespace Voting_0._2.Controllers
{
    public class AuthController : Controller
    {
        private const string AdminAccessCode = "AdminAccess123"; // Статичний код доступу для адміністратора
        private readonly VotingDbContext _context;

        public AuthController(VotingDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult AccessDenied(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost("enter-admin-access-code")]
        public IActionResult EnterAdminAccessCode(string accessCode)
        {
            // Виведення введеного коду доступу в консоль для перевірки
            Console.WriteLine($"Введений код доступу: '{accessCode}'");

            if (accessCode == AdminAccessCode)
            {
                // Якщо код правильний, перенаправляємо користувача
                return Json(new { isValid = true, redirectUrl = Url.Action("LoginAdmin", "AdminAccount") });
            }

            // Якщо код неправильний, повертаємо JSON з помилкою
            return Json(new { isValid = false });
        }

        // Вхід для користувачів через нікнейм і код доступу
        [HttpGet("login-voter")]
        public IActionResult LoginVoter()
        {
            return View();
        }

        [HttpPost("login-voter")]
        public IActionResult LoginVoter(string nickname, string accessCode)
        {
            if (string.IsNullOrEmpty(accessCode))
            {
                ModelState.AddModelError(string.Empty, "Код доступу обов'язковий.");
                return View();
            }

            // Перевірка коду доступу до голосування
            var voting = _context.Votings.FirstOrDefault(v => v.AccessKey == accessCode);
            if (voting == null)
            {
                ModelState.AddModelError(string.Empty, "Невірний код доступу.");
                return View();
            }

            // Якщо нікнейм не вказано, вхід відбувається анонімно
            if (string.IsNullOrEmpty(nickname))
            {
                nickname = "Anonymous";
            }

            // Збереження нікнейму та коду доступу у сесії для подальшого використання
            HttpContext.Session.SetString("VoterNickname", nickname);
            HttpContext.Session.SetString("VotingAccessKey", accessCode);

            // Перенаправляємо користувача на сторінку голосування
            return RedirectToAction("Vote", new { id = voting.Id });
        }
    }
}

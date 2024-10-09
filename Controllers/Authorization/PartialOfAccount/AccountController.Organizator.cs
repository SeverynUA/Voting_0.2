using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Voting_0._2.Data.Entities.Users;
using Voting_0._2.Models.DTOs.Account;
using Voting_0._2.Models.Voting_m.Candidat_m;
using Voting_0._2.Models.Voting_m.SetUp;
using Voting_0._2.Models.Voting_m;

namespace Voting_0._2.Controllers.Authorization
{
    public partial class AccountController : Controller
    {
        readonly string html_pathOrganizator = "~/Views/Account/Organizator/";

        [Authorize(Roles = Roles.Organizator)]
        [HttpGet("Organizator/Index")]
        public async Task<IActionResult> OrganizatorIndex()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Користувача не знайдено.");
                return NotFound("Користувача не знайдено.");
            }
            return View($"{html_pathOrganizator}OrganizatorIndex.cshtml", user);
        }

        // Сторінка реєстрації організатора
        [HttpGet("register-organizator")]
        public IActionResult RegisterOrganizator()
        {
            return View($"{html_pathOrganizator}RegisterOrganizator.cshtml");
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
                        // Логіка додавання тестових голосувань під час реєстрації організатора
                        await AddTestVotingsForOrganizator(organizator);

                        // Після успішної реєстрації перенаправляємо на сторінку входу
                        return RedirectToAction("LoginOrganizator", "Account");
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
            return View($"{html_pathOrganizator}RegisterOrganizator.cshtml" , model);
        }

        // Метод для додавання тестових голосувань
        private async Task AddTestVotingsForOrganizator(Account organizator)
        {
            // Створення голосувань для організатора
            var voting1 = new Voting
            {
                Name = "Вибори президента",
                AccessKey = "PREZ2024",
                VotingDuration = TimeSpan.FromHours(3),
                NumberOfVoters = 2,
                Organizator = organizator,
                OrganizatorId = organizator.Id, // Прив'язка до зареєстрованого організатора
                VotingSystem = new VotingSystem(VotingMode.Standard),
                Candidates = new List<Candidat>
            {
                new Candidat { Name = "Кандидат 1", Description = "Опис кандидата 1", VoteCount = 0 },
                new Candidat { Name = "Кандидат 2", Description = "Опис кандидата 2", VoteCount = 0 }
            }
            };

            var voting2 = new Voting
            {
                Name = "Мер міста",
                AccessKey = "MAYOR2024",
                VotingDuration = TimeSpan.FromHours(2),
                NumberOfVoters = null, // Відкрите голосування
                Organizator = organizator,
                OrganizatorId = organizator.Id,
                VotingSystem = new VotingSystem(VotingMode.Elimination),
                Candidates = new List<Candidat>
            {
                new Candidat { Name = "Кандидат A", Description = "Опис кандидата A", VoteCount = 0 },
                new Candidat { Name = "Кандидат B", Description = "Опис кандидата B", VoteCount = 0 }
            }
            };

            // Додаємо голосування до контексту
            _dbContext.Votings.AddRange(voting1, voting2);

            // Зберігаємо зміни в базі даних
            await _dbContext.SaveChangesAsync();
        }

        // Сторінка входу для організатора
        [HttpGet("login-organizator")]
        public IActionResult LoginOrganizator()
        {
            return View($"{html_pathOrganizator}LoginOrganizator.cshtml");
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
                    return RedirectToAction("OrganizatorIndex", "Account");
                }
                ModelState.AddModelError(string.Empty, "Невірний логін або пароль");
            }
            return View($"{html_pathOrganizator}LoginOrganizator.cshtml" , model);
        }

        // Сторінка редагування облікового запису організатора
        [Authorize(Roles = Roles.Organizator)]
        [HttpGet("edit-account-organizator")]
        public IActionResult EditAccountOrganizator()
        {
            return View($"{html_pathOrganizator}EditAccountOrganizator.cshtml");
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

            return RedirectToAction($"{html_pathOrganizator}OrganizatorIndex.cshtml", "OrganizatorAccount");
        }

        // Сторінка зміни пароля організатора
        [Authorize(Roles = Roles.Organizator)]
        [HttpGet("change-password-organizator")]
        public IActionResult ChangePasswordOrganizator()
        {
            return View($"{html_pathOrganizator}ChangePasswordOrganizator.cshtml", new ChangePasswordModel());
        }

        [Authorize(Roles = Roles.Organizator)]
        [HttpPost("change-password-organizator")]
        public async Task<IActionResult> ChangePasswordOrganizator(ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction($"{html_pathOrganizator}LoginOrganizator.cshtml", "OrganizatorAccount");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                return RedirectToAction($"{html_pathOrganizator}OrganizatorIndex.cshtml", "OrganizatorAccount");
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

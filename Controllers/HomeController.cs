using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Voting_0._2.Data.Entities.Users;
using Voting_0._2.Models;

namespace Voting_0._2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole(Roles.Admin))
                {
                    return RedirectToAction("Index", "AdminAccount");
                }
                if (User.IsInRole(Roles.Organizator))
                {
                    return RedirectToAction("Index", "OrganizatorAccount");
                }
                if (User.IsInRole(Roles.Voter))
                {
                    return View();
                }
                else
                {
                    HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

                    // Якщо користувач не має відповідної ролі
                    return RedirectToAction("LoginVoter", "Auth");
                }
            }
            else
            {
                // Користувач не зареєстрований, перенаправляємо на сторінку логіну
                return RedirectToAction("LoginVoter", "Auth");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

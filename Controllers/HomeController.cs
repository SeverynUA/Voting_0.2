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
            ////Поки не рішиться проблема із переходом до головної сторінки

            ///////////////////////
            //HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            ///////////////////////

            if (User.Identity.IsAuthenticated)
            {
                _logger.LogInformation("User is authenticated.");
                if (User.IsInRole(Roles.Admin))
                {
                    _logger.LogInformation("User is in Admin role.");
                    return RedirectToAction("Index", "AdminAccount");
                }
                if (User.IsInRole(Roles.Organizator))
                {
                    _logger.LogInformation("User is in Organizator role.");
                    return RedirectToAction("Index", "OrganizatorAccount");
                }
                else
                {
                    _logger.LogInformation("User has no valid role.");
                    HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                    return RedirectToAction("LoginVoter", "Auth");
                }
            }
            else
            {
                _logger.LogInformation("User is not authenticated.");
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

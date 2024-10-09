using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Voting_0._2.Data.Entities.Users;
using Voting_0._2.Data.Entities;

namespace Voting_0._2.Controllers.Authorization
{

    [Route("Account")]
    public partial class AccountController : Controller
    {
        private readonly UserManager<Account> _userManager;
        private readonly SignInManager<Account> _signInManager;
        private readonly VotingDbContext _dbContext;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<Account> userManager, SignInManager<Account> signInManager, VotingDbContext dbContext, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult AccessDenied(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
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

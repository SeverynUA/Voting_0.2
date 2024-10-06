using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using Voting_0._2.Data.Entities;
using Voting_0._2.Models.Voting_m.SetUp;
using Voting_0._2.Models.Voting_m;
using Microsoft.EntityFrameworkCore;
using Voting_0._2.Models.Voting_m.Candidat_m;
using Voting_0._2.Models.ViewModels;
using Voting_0._2.Data.Entities.Users;
using Voting_0._2.Models.ViewModels.CreateModels;
using Microsoft.AspNetCore.Identity;

namespace Voting_0._2.Controllers
{
    public class VotingController : Controller
    {
        private readonly VotingDbContext _dbContext;
        private readonly UserManager<Account> _userManager;

        public VotingController(UserManager<Account> userManager ,VotingDbContext dbContext)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        // Список голосувань для перегляду з можливістю редагування та видалення
        [HttpGet("list")]
        [Authorize(Roles = "Organizator")]
        public async Task<IActionResult> GetVotings()
        {
            var currentUserId = _userManager.GetUserId(User); // Отримуємо Id поточного користувача
            var votings = await _dbContext.Votings
                .Where(v => v.OrganizatorId == currentUserId) // Фільтруємо голосування за організатором
                .ToListAsync();

            return View(votings);
        }


        [HttpGet("{votingId}/edit")]
        [Authorize(Roles = "Organizator")]
        public async Task<IActionResult> EditVoting(int votingId)
        {
            var voting = await _dbContext.Votings.FindAsync(votingId);
            if (voting == null) return NotFound();

            return View(voting);
        }


        [HttpPost("{votingId}/edit")]
        [Authorize(Roles = "Organizator")]
        public async Task<IActionResult> EditVoting(int votingId, Voting model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var voting = await _dbContext.Votings.FindAsync(votingId);
            if (voting == null) return NotFound();

            voting.Name = model.Name;
            voting.VotingDuration = model.VotingDuration;
            voting.AccessKey = model.AccessKey;
            voting.NumberOfVoters = model.NumberOfVoters;

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("GetVotings");
        }


        [HttpGet("{votingId}/delete")]
        [Authorize(Roles = "Organizator")]
        public async Task<IActionResult> DeleteVoting(int votingId)
        {
            var voting = await _dbContext.Votings.FindAsync(votingId);
            if (voting == null) return NotFound();

            return View(voting); // Сторінка з підтвердженням видалення
        }

        [HttpPost("{votingId}/delete")]
        [Authorize(Roles = "Organizator")]
        public async Task<IActionResult> ConfirmDeleteVoting(int votingId)
        {
            var voting = await _dbContext.Votings.FindAsync(votingId);
            if (voting == null) return NotFound();

            _dbContext.Votings.Remove(voting);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("GetVotings");
        }


        [HttpGet("create")]
        [Authorize(Roles = "Organizator")]
        public IActionResult CreateVoting()
        {
            return View();
        }

        [HttpPost("create")]
        [Authorize(Roles = "Organizator")]
        public async Task<IActionResult> CreateVoting(VotingCreateModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var votingSystem = new VotingSystem(model.Mode);

            var voting = new Voting
            {
                Name = model.Name,
                VotingSystem = votingSystem,
                VotingDuration = model.VotingDuration,
                AccessKey = model.AccessKey,
                NumberOfVoters = model.NumberOfVoters,
                Organizator = await _userManager.GetUserAsync(User)
            };

            _dbContext.Votings.Add(voting);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("GetVotings");
        }

        // Сторінка для додавання кандидата (тільки для Organizator)
        [HttpGet("{votingId}/add-candidate")]
        [Authorize(Roles = "Organizator")]
        public IActionResult AddCandidate(int votingId)
        {
            ViewBag.VotingId = votingId;
            return View();
        }

        [HttpPost("{votingId}/add-candidate")]
        [Authorize(Roles = "Organizator")]
        public async Task<IActionResult> AddCandidateToVoting(int votingId, CandidateCreateModel model)
        {
            var voting = await _dbContext.Votings.Include(v => v.Candidates).FirstOrDefaultAsync(v => v.Id == votingId);
            if (voting == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.VotingId = votingId;
                return View(model);
            }

            if (voting.Candidates.Any(c => c.Name == model.Name))
            {
                ModelState.AddModelError("", "Кандидат з таким іменем вже існує.");
                ViewBag.VotingId = votingId;
                return View(model);
            }

            var candidate = new Candidat { Name = model.Name };
            voting.Candidates.Add(candidate);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Details", new { votingId });
        }

        // Сторінка для перегляду деталей голосування
        [HttpGet("{votingId}")]
        [Authorize(Roles = "Admin, Voter")]
        public async Task<IActionResult> Details(int votingId)
        {
            var voting = await _dbContext.Votings.Include(v => v.Candidates).FirstOrDefaultAsync(v => v.Id == votingId);
            if (voting == null) return NotFound();

            return View(voting);
        }

        // Сторінка для запуску голосування (тільки для Organizator)
        [HttpGet("{votingId}/start")]
        [Authorize(Roles = "Organizator")]
        public async Task<IActionResult> StartVoting(int votingId)
        {
            var voting = await _dbContext.Votings.FindAsync(votingId);
            if (voting == null) return NotFound();

            return View(voting);
        }

        [HttpPost("{votingId}/start")]
        [Authorize(Roles = "Organizator")]
        public async Task<IActionResult> StartVotingConfirmed(int votingId)
        {
            var voting = await _dbContext.Votings.FindAsync(votingId);
            if (voting == null) return NotFound();

            if (voting.IsActive)
            {
                return BadRequest("Голосування вже розпочато.");
            }

            await voting.StartVotingAsync();
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Details", new { votingId });
        }

        // Сторінка для перегляду результатів голосування (тільки для Organizator)
        [HttpGet("{votingId}/results")]
        [Authorize(Roles = "Organizator")]
        public async Task<IActionResult> GetVotingResults(int votingId)
        {
            var voting = await _dbContext.Votings
                .Include(v => v.Candidates)
                .FirstOrDefaultAsync(v => v.Id == votingId);

            if (voting == null) return NotFound();

            return View(voting);
        }

        [HttpPost("{votingId}/cast-vote")]
        [Authorize(Roles = "Voter")]
        public async Task<IActionResult> CastVote(int votingId, VoteModel model)
        {
            var voting = await _dbContext.Votings.Include(v => v.Candidates).Include(v => v.Voters).FirstOrDefaultAsync(v => v.Id == votingId);
            if (voting == null) return NotFound();

            if (!voting.IsActive)
            {
                return BadRequest("Голосування не активне.");
            }

            var candidate = voting.Candidates.FirstOrDefault(c => c.Id == model.CandidateId);
            if (candidate == null) return NotFound("Кандидат не знайдений.");

            // Перевіряємо, чи цей виборець вже голосував
            var existingVote = voting.Voters.FirstOrDefault(v => v.Nickname == model.Nickname);
            if (existingVote != null)
            {
                return BadRequest("Ви вже проголосували.");
            }

            // Визначаємо ім'я виборця (якщо анонімно, то "Анонім", або якщо нікнейм не вказано - "Невідомий")
            string voterName = model.IsAnonymous ? "Анонім" : model.Nickname ?? "Невідомий";

            // Створюємо новий запис про виборця (Voter_User)
            var voter = new Voter_User
            {
                Nickname = voterName,
                VotingId = votingId,
                Voting = voting
            };

            // Створюємо новий запис про голос
            var vote = new Vote
            {
                CandidateId = model.CandidateId,
                VoterName = voterName,
                VoteTime = DateTime.Now
            };

            voting.Voters.Add(voter);
            candidate.VoteCount += 1; // Збільшуємо лічильник голосів кандидата

            _dbContext.Votes.Add(vote);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Details", new { votingId });
        }
    }
}

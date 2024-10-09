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
using Voting_0._2.Models.ViewModels.EditModel;
using Voting_0._2.Models.ViewModels.DetailsModel;

namespace Voting_0._2.Controllers.SetUp_Voting
{
    public class VotingController : Controller
    {
        private readonly VotingDbContext _dbContext;
        private readonly UserManager<Account> _userManager;

        public VotingController(UserManager<Account> userManager, VotingDbContext dbContext)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        // Список голосувань для перегляду з можливістю редагування та видалення
        [HttpGet("list")]
        [Authorize(Roles = Roles.Organizator)]
        public async Task<IActionResult> GetVotings()
        {
            var currentUserId = _userManager.GetUserId(User); // Отримуємо Id поточного користувача
            var votings = await _dbContext.Votings
                .Where(v => v.OrganizatorId == currentUserId) // Фільтруємо голосування за організатором
                .ToListAsync();

            return View(votings);
        }


        // GET: Редагування голосування
        [HttpGet("{votingId}/edit")]
        [Authorize(Roles = Roles.Organizator)]
        public async Task<IActionResult> EditVoting(int votingId)
        {
            var voting = await _dbContext.Votings
                .Include(v => v.Candidates)
                .FirstOrDefaultAsync(v => v.Id == votingId);

            if (voting == null) return NotFound();

            // Мапінг даних з Voting до VotingEditModel
            var model = new VotingEditModel
            {
                Name = voting.Name,
                VotingDuration = voting.VotingDuration,
                AccessKey = voting.AccessKey,
                NumberOfVoters = voting.NumberOfVoters,
                Candidates = voting.Candidates.Select(c => new CandidateEditModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                }).ToList()
            };

            return View(model);
        }

        // POST: Збереження змін голосування
        [HttpPost("{votingId}/edit")]
        [Authorize(Roles = Roles.Organizator)]
        public async Task<IActionResult> EditVoting(int votingId, VotingEditModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var voting = await _dbContext.Votings
                .Include(v => v.Candidates)
                .FirstOrDefaultAsync(v => v.Id == votingId);

            if (voting == null) return NotFound();

            // Оновлюємо основні дані голосування
            voting.Name = model.Name;
            voting.VotingDuration = model.VotingDuration;
            voting.AccessKey = model.AccessKey;
            voting.NumberOfVoters = model.NumberOfVoters;

            // Оновлюємо кандидатів
            foreach (var candidateModel in model.Candidates)
            {
                var existingCandidate = voting.Candidates.FirstOrDefault(c => c.Id == candidateModel.Id);
                if (existingCandidate != null)
                {
                    existingCandidate.Name = candidateModel.Name;
                    existingCandidate.Description = candidateModel.Description;

                    // Якщо кандидат має нове зображення
                    if (candidateModel.ImageFile != null)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await candidateModel.ImageFile.CopyToAsync(memoryStream);
                            existingCandidate.Image = new Image
                            {
                                ImageData = memoryStream.ToArray(),
                                CandidatID = existingCandidate.Id
                            };
                        }
                    }
                }
                else
                {
                    // Якщо кандидат новий, додаємо його
                    voting.Candidates.Add(new Candidat
                    {
                        Name = candidateModel.Name,
                        Description = candidateModel.Description
                    });
                }
            }

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("GetVotings", "VotingController");
        }

        [HttpGet("{votingId}/delete")]
        [Authorize(Roles = Roles.Organizator)]
        public async Task<IActionResult> DeleteVoting(int votingId)
        {
            var voting = await _dbContext.Votings.FindAsync(votingId);
            if (voting == null) return NotFound();

            return View(voting); // Сторінка з підтвердженням видалення
        }

        [HttpPost("{votingId}/delete")]
        [Authorize(Roles = Roles.Organizator)]
        public async Task<IActionResult> ConfirmDeleteVoting(int votingId)
        {
            var voting = await _dbContext.Votings.FindAsync(votingId);
            if (voting == null) return NotFound();

            _dbContext.Votings.Remove(voting);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("GetVotings", "VotingController");
        }


        [HttpGet("create")]
        [Authorize(Roles = Roles.Organizator)]
        public IActionResult CreateVoting()
        {
            return View();
        }

        [HttpPost("create")]
        [Authorize(Roles = Roles.Organizator)]
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

            return RedirectToAction("GetVotings", "VotingController");
        }

        [HttpPost]
        [Authorize(Roles = Roles.Organizator)]
        public async Task<IActionResult> AddCandidates(List<CandidateCreateModel> candidates, int votingId)
        {
            var voting = await _dbContext.Votings.Include(v => v.Candidates).FirstOrDefaultAsync(v => v.Id == votingId);
            if (voting == null) return NotFound();

            foreach (var candidateModel in candidates)
            {
                var candidate = new Candidat
                {
                    Name = candidateModel.Name,
                    Description = candidateModel.Description,
                    Voting = voting
                };

                if (candidateModel.ImageFile != null && candidateModel.ImageFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await candidateModel.ImageFile.CopyToAsync(memoryStream);
                        var image = new Image
                        {
                            ImageData = memoryStream.ToArray(),
                            Candidat = candidate
                        };
                        _dbContext.Images.Add(image);
                    }
                }

                _dbContext.Candidates.Add(candidate);
            }

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Details", new { id = votingId });
        }

        [HttpPost("{votingId}/add-candidate")]
        [Authorize(Roles = Roles.Organizator)]
        public async Task<IActionResult> AddCandidateToVoting(int votingId, CandidateCreateModel model)
        {
            // Знаходимо голосування
            var voting = await _dbContext.Votings.Include(v => v.Candidates).FirstOrDefaultAsync(v => v.Id == votingId);
            if (voting == null)
            {
                return NotFound("Голосування не знайдено.");
            }

            // Валідація моделі
            if (!ModelState.IsValid)
            {
                ViewBag.VotingId = votingId;
                return View(model);
            }

            // Перевіряємо, чи вже є кандидат з таким ім'ям
            if (voting.Candidates.Any(c => c.Name == model.Name))
            {
                ModelState.AddModelError("", "Кандидат з таким іменем вже існує.");
                ViewBag.VotingId = votingId;
                return View(model);
            }

            // Створюємо нового кандидата
            var candidate = new Candidat
            {
                Name = model.Name,
                Voting = voting // Встановлюємо зв'язок через навігаційну властивість
            };

            // Додаємо кандидата до колекції кандидатів голосування
            voting.Candidates.Add(candidate);
            await _dbContext.SaveChangesAsync(); // Зберігаємо зміни

            return RedirectToAction("Details", new { id = votingId });
        }


        [HttpPost]
        [Authorize(Roles = Roles.Organizator)]
        public async Task<IActionResult> AddCandidateWithImage(CandidateCreateModel model, IFormFile imageFile, int votingId)
        {
            // Валідація моделі
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Знаходимо голосування
            var voting = await _dbContext.Votings.Include(v => v.Candidates).FirstOrDefaultAsync(v => v.Id == votingId);
            if (voting == null)
            {
                return NotFound("Голосування не знайдено.");
            }

            // Створюємо нового кандидата
            var candidate = new Candidat
            {
                Name = model.Name,
                Description = model.Description,
                Voting = voting // Встановлюємо зв'язок через навігаційну властивість
            };

            // Обробка зображення, якщо воно додане
            if (imageFile != null && imageFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await imageFile.CopyToAsync(memoryStream);
                    var image = new Image
                    {
                        ImageData = memoryStream.ToArray(),
                        Candidat = candidate // Встановлюємо зв'язок із кандидатом
                    };
                    _dbContext.Images.Add(image);
                }
            }

            // Додаємо кандидата до колекції
            voting.Candidates.Add(candidate);
            await _dbContext.SaveChangesAsync(); // Зберігаємо зміни

            return RedirectToAction("Details", new { id = votingId });
        }



        // Сторінка для перегляду деталей голосування
        [HttpGet("{votingId}")]
        public async Task<IActionResult> Details(int votingId)
        {
            // Завантажуємо голосування з кандидатами
            var voting = await _dbContext.Votings
                .Include(v => v.Candidates)
                .FirstOrDefaultAsync(v => v.Id == votingId);

            if (voting == null) return NotFound("Голосування не знайдено.");

            // Мапимо дані на модель для перегляду деталей
            var model = new VotingDetailsModel
            {
                Name = voting.Name,
                VotingDuration = voting.VotingDuration,
                AccessKey = voting.AccessKey,
                NumberOfVoters = voting.NumberOfVoters,
                Candidates = voting.Candidates.Select(c => new CandidateDetailsModel
                {
                    Name = c.Name,
                    Description = c.Description,
                    VoteCount = c.VoteCount,
                    ImageUrl = c.Image != null ? ConvertImageToBase64(c.Image.ImageData) : null
                }).ToList()
            };

            // Передаємо модель в представлення
            return View(model);
        }

        private string ConvertImageToBase64(byte[] imageData)
        {
            return "data:image/png;base64," + Convert.ToBase64String(imageData);
        }

        // Сторінка для запуску голосування (тільки для Organizator)
        [HttpGet("{votingId}/start")]
        [Authorize(Roles = Roles.Organizator)]
        public async Task<IActionResult> StartVoting(int votingId)
        {
            var voting = await _dbContext.Votings.FindAsync(votingId);
            if (voting == null) return NotFound();

            return View(voting);
        }

        [HttpPost("{votingId}/start")]
        [Authorize(Roles = Roles.Organizator)]
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
        [Authorize(Roles = Roles.Organizator)]
        public async Task<IActionResult> GetVotingResults(int votingId)
        {
            var voting = await _dbContext.Votings
                .Include(v => v.Candidates)
                .FirstOrDefaultAsync(v => v.Id == votingId);

            if (voting == null) return NotFound();

            return View(voting);
        }

        [HttpPost("{votingId}/cast-vote")]
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

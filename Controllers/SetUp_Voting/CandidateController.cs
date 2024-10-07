using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Voting_0._2.Data.Entities;
using Voting_0._2.Models.ViewModels.CreateModels;
using Voting_0._2.Models.Voting_m.Candidat_m;

namespace Voting_0._2.Controllers.SetUp_Voting
{
    public class CandidateController : Controller
    {
        private readonly VotingDbContext _dbContext;

        public CandidateController(VotingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("create-candidate")]
        public IActionResult CreateCandidate()
        {
            return View();
        }

        [HttpPost("create-candidate")]
        public async Task<IActionResult> CreateCandidate(CandidateCreateModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var candidate = new Candidat
            {
                Name = model.Name,
                Description = model.Description,
                VoteCount = 0
            };

            _dbContext.Candidates.Add(candidate);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("CandidateDetails", new { candidateId = candidate.Id });
        }

        [HttpGet("candidate-details/{candidateId}")]
        public async Task<IActionResult> CandidateDetails(int candidateId)
        {
            var candidate = await _dbContext.Candidates
                .Include(c => c.Image) // Підключаємо зображення кандидата
                .FirstOrDefaultAsync(c => c.Id == candidateId);

            if (candidate == null)
            {
                return NotFound();
            }

            return View(candidate);
        }
    }
}

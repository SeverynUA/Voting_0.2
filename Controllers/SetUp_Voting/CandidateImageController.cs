using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Voting_0._2.Data.Entities;
using Voting_0._2.Models.Voting_m.Candidat_m;

namespace Voting_0._2.Controllers.SetUp_Voting
{
    public class CandidateImageController : Controller
    {
        private readonly VotingDbContext _dbContext;

        public CandidateImageController(VotingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Сторінка для додавання фото до кандидата
        [HttpGet("add-candidate-image")]
        [Authorize(Roles = "Organizator")]
        public IActionResult AddCandidateImage(int candidateId)
        {
            ViewBag.CandidateId = candidateId;
            return View();
        }

        [HttpPost("add-candidate-image")]
        [Authorize(Roles = "Organizator")]
        public async Task<IActionResult> AddCandidateImage(int candidateId, IFormFile imageFile)
        {
            var candidate = await _dbContext.Candidats.Include(c => c.Image).FirstOrDefaultAsync(c => c.Id == candidateId);
            if (candidate == null)
            {
                return NotFound();
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await imageFile.CopyToAsync(memoryStream);

                var image = new Image
                {
                    ImageData = memoryStream.ToArray(),
                    FilePath = imageFile.FileName,
                    CandidatID = candidateId
                };

                candidate.Image = image;
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Candidate", new { candidateId });
        }
    }

}

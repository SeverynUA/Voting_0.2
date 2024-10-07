using Voting_0._2.Data.Entities;
using Voting_0._2.Models.Voting_m.Candidat_m;

namespace Voting_0._2.Service
{
    public class CandidateService
    {
        private readonly VotingDbContext _context;

        public CandidateService(VotingDbContext context)
        {
            _context = context;
        }

        // Метод для додавання кандидата з зображенням
        public async Task AddCandidatWithImageAsync(string name, string description, byte[] imageData)
        {
            var candidat = new Candidat
            {
                Name = name,
                Description = description,
                Image = new Image
                {
                    ImageData = imageData
                }
            };

            _context.Candidates.Add(candidat);
            await _context.SaveChangesAsync();
        }
    }

}

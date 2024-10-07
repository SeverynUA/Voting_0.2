using System.ComponentModel.DataAnnotations;

namespace Voting_0._2.Models.ViewModels.CreateModels
{
    public class CandidateCreateModel
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public IFormFile ImageFile { get; set; } // Поле для завантаження зображення

        public int VotingId { get; set; } // Додаємо поле для ідентифікації голосування
    }

}

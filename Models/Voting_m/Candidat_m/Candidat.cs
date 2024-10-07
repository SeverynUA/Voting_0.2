using System.ComponentModel.DataAnnotations;

namespace Voting_0._2.Models.Voting_m.Candidat_m
{
    public class Candidat
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int VoteCount { get; set; } = 0;
        public Image? Image { get; set; } // Навігаційна властивість до зображення

        // Навігаційна властивість для зв'язку з Voting
        public Voting Voting { get; set; } = default!;
    }
}

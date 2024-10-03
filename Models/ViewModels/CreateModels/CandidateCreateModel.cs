using System.ComponentModel.DataAnnotations;

namespace Voting_0._2.Models.ViewModels.CreateModels
{
    public class CandidateCreateModel
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}

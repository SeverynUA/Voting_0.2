namespace Voting_0._2.Models.ViewModels.DetailsModel
{
    public class CandidateDetailsModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int VoteCount { get; set; }
        public string ImageUrl { get; set; } // URL або Base64-строка зображення
    }
}

namespace Voting_0._2.Models.ViewModels
{
    public class Vote
    {
        public int Id { get; set; }
        public int CandidateId { get; set; }
        public string VoterName { get; set; } // Нікнейм або "Анонім"
        public DateTime VoteTime { get; set; } = DateTime.Now; // Час голосування
    }

}

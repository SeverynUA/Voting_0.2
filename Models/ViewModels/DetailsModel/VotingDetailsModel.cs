namespace Voting_0._2.Models.ViewModels.DetailsModel
{
    public class VotingDetailsModel
    {
        public string Name { get; set; }
        public TimeSpan VotingDuration { get; set; }
        public string AccessKey { get; set; }
        public int? NumberOfVoters { get; set; }

        public List<CandidateDetailsModel> Candidates { get; set; } = new List<CandidateDetailsModel>();
    }
}

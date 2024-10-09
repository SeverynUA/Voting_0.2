namespace Voting_0._2.Models.ViewModels.EditModel
{
    public class VotingEditModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TimeSpan VotingDuration { get; set; }
        public string AccessKey { get; set; }
        public int? NumberOfVoters { get; set; }

        public List<CandidateEditModel> Candidates { get; set; } = new List<CandidateEditModel>();
    }
}

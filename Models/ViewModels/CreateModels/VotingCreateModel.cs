using Voting_0._2.Data.Entities.Users;
using Voting_0._2.Models.Voting_m.SetUp;

namespace Voting_0._2.Models.ViewModels.CreateModels
{
    public class VotingCreateModel
    {
        public string Name { get; set; }
        public string AccessKey { get; set; }
        public VotingMode Mode { get; set; }
        public TimeSpan VotingDuration { get; set; }
        public int? NumberOfVoters { get; set; }
        public Account Organizator { get; set; }
    }
}

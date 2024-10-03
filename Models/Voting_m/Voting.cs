using Microsoft.AspNetCore.Routing.Matching;
using System.Xml.Linq;
using Voting_0._2.Data.Entities.Users;
using Voting_0._2.Models.Voting_m.Candidat_m;
using Voting_0._2.Models.Voting_m.SetUp;

namespace Voting_0._2.Models.Voting_m
{
    public class Voting
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public VotingSystem VotingSystem { get; set; }
        public TimeSpan VotingDuration { get; set; }
        public string AccessKey { get; set; }
        public int? NumberOfVoters { get; set; }

        public string OrganizatorId { get; set; }
        public Account Organizator { get; set; }

        public List<Voter_User> Voters { get; set; } = new List<Voter_User>();
        public List<Candidat> Candidates { get; set; } = new List<Candidat>();

        public bool IsActive { get; private set; } = false;

        // Метод для запуску голосування
        public async Task StartVotingAsync()
        {
            if (IsActive) return;
            IsActive = true;
            await Task.Delay(VotingDuration);
            EndVoting();
        }

        public void EndVoting()
        {
            IsActive = false;
        }
    }
}

using System.ComponentModel.DataAnnotations;
using Voting_0._2.Models.ViewModels;
using Voting_0._2.Models.Voting_m;

namespace Voting_0._2.Data.Entities.Users
{
    public class Voter_User
    {
        public int Id { get; set; }
        public string Nickname { get; set; } // Нікнейм виборця

        // Виборець пов'язаний із голосуванням
        public int VotingId { get; set; }
        public Voting Voting { get; set; }

        // Зв'язок із голосами
        public Vote Vote { get; set; } // Один виборець може зробити один голос
    }


}

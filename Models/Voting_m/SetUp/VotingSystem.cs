using Microsoft.AspNetCore.Routing.Matching;
using System.Xml.Linq;
using Voting_0._2.Models.Voting_m.Candidat_m;

namespace Voting_0._2.Models.Voting_m.SetUp
{
    public class VotingSystem
    {
        public VotingMode Mode { get; set; }

        public VotingSystem(VotingMode mode)
        {
            Mode = mode;
        }

        // Логіка для визначення переможця
        public Candidat DetermineWinner(List<Candidat> candidates)
        {
            return Mode == VotingMode.Elimination ? DetermineEliminationWinner(candidates) : DetermineStandardWinner(candidates);
        }

        private Candidat DetermineStandardWinner(List<Candidat> candidates)
        {
            return candidates.OrderByDescending(c => c.VoteCount).FirstOrDefault();
        }

        private Candidat DetermineEliminationWinner(List<Candidat> candidates)
        {
            while (candidates.Count > 1)
            {
                candidates = candidates.OrderBy(c => c.VoteCount).ToList();
                int removeCount = candidates.Count / 2;
                candidates.RemoveRange(0, removeCount);
            }

            return candidates.FirstOrDefault();
        }
    }

}

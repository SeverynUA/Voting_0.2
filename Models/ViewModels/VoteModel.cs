namespace Voting_0._2.Models.ViewModels
{
    public class VoteModel
    {
        public int CandidateId { get; set; } // Кандидат, за якого голосують
        public string? Nickname { get; set; } // Нікнейм користувача, якщо не анонімно
        public bool IsAnonymous { get; set; } // Прапорець для анонімного голосування
    }

}

using System.ComponentModel.DataAnnotations;

namespace Voting_0._2.Models.DTOs.Voter
{
    public class LoginVoter
    {
        public string? Nickname { get; set; }

        [Required(ErrorMessage = "Пароль обов'язковий.")]
        public string Key_Password { get; set; }

    }
}

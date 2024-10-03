using System.ComponentModel.DataAnnotations;

namespace Voting_0._2.Models.DTOs.Account
{
    public class EditAccountModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FullName { get; set; }

        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Нові паролі не співпадають")]
        [DataType(DataType.Password)]
        public string ConfirmNewPassword { get; set; }
    }
}

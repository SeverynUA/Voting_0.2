using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Voting_0._2.Models.Voting_m;

namespace Voting_0._2.Data.Entities.Users
{
    public class Account : IdentityUser
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }
    }
}

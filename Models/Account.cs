using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VierGewinnt.Models
{
    public class Account : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        //public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Playername")]
        [Required(ErrorMessage = "required")]
        public string? PlayerName { get; set; }

        //[Display(Name = "E-Mail")]
        //[Required(ErrorMessage = "required")]
        //public string? Email { get; set; }

        [Display(Name = "Password")]
        [Required(ErrorMessage = "required")]
        public string Password { get; set; }
    }
}

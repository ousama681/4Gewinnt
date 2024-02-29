using VierGewinnt.Models;
using System.ComponentModel.DataAnnotations;

namespace VierGewinnt.ViewModels
{
    public class UserManagamentViewModel
    {

        public string Id { get; set; }

        [Display(Name = "Playername")]
        [Required(ErrorMessage = "required")]
        public string? PlayerName { get; set; }

        [Display(Name = "E-Mail")]
        [Required(ErrorMessage = "required")]
        public string? Email { get; set; }

        [Display(Name = "Password")]
        [Required(ErrorMessage = "required")]
        public string Password { get; set; }
        public string Pagetitle { get; set; }



        public UserManagamentViewModel(Account acc)
        {
            Id = acc.Id;
            PlayerName = acc.PlayerName;
            Email = acc.Email;
            Password = acc.Password;
        }
    }
}

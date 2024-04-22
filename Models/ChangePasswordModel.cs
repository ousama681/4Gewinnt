using System.ComponentModel.DataAnnotations;

namespace VierGewinnt.Models
{
    public class ChangePasswordModel
    {
        [Required, DataType(DataType.Password), Display(Name = "Current password")]
        public string CurrentPassword { get; set; }

        [Required, DataType(DataType.Password), Display(Name = "New password")]
        public string NewPassword { get; set; }
    }
}

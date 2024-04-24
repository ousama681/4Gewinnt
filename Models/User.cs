using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VierGewinnt.Models
{
    public class User : IdentityUser
    {
        bool InGame { get; set; }

    }
}

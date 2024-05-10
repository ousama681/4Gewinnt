using Microsoft.AspNetCore.Identity;
using System.Configuration;
using VierGewinnt.Data.Models;
using VierGewinnt.Models;

namespace VierGewinnt.Data.Interfaces
{
    public interface IAccountRepository : IRepository<ApplicationUser>
    {

        Task<ApplicationUser> GetUserByUsername(string username);
        Task<ApplicationUser> GetUserByEmailAsync(string email);

        Task<IdentityResult> CreateUserAsync(SignUpUserModel userModel);

        Task<SignInResult> PasswordSignInAsync(SignInModel signInModel);

        Task SignOutAsync();

        Task<IdentityResult> ChangePasswordAsync(ChangePasswordModel model);

        Task<IdentityResult> ConfirmEmailAsync(string uid, string token);

        Task GenerateEmailConfirmationTokenAsync(ApplicationUser user);

        //Task GenerateForgotPasswordTokenAsync(ApplicationUser user);

        //Task<IdentityResult> ResetPasswordAsync(ResetPasswordModel model);
    }
}

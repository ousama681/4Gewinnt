using VierGewinnt.Models;
using Microsoft.AspNetCore.Identity;
using System.Configuration;

namespace VierGewinnt.Repositories.Interfaces
{
    public interface IAccountRepository : IRepository<IdentityUser>
    {
        Task<IdentityUser> GetUserByEmailAsync(string email);

        Task<IdentityResult> CreateUserAsync(SignUpUserModel userModel);

        Task<SignInResult> PasswordSignInAsync(SignInModel signInModel);

        Task SignOutAsync();

        Task<IdentityResult> ChangePasswordAsync(ChangePasswordModel model);

        Task<IdentityResult> ConfirmEmailAsync(string uid, string token);

        Task GenerateEmailConfirmationTokenAsync(IdentityUser user);

        //Task GenerateForgotPasswordTokenAsync(IdentityUser user);

        //Task<IdentityResult> ResetPasswordAsync(ResetPasswordModel model);
    }
}

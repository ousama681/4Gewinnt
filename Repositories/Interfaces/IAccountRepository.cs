using VierGewinnt.Models;
using Microsoft.AspNetCore.Identity;
using System.Configuration;

namespace VierGewinnt.Repositories.Interfaces
{
    public interface IAccountRepository : IRepository<Account>
    {
        //public Task<Account> GetByEmailAsync(Account item);

        //public Task SendEmailConfirmationEmail(IdentityUser user, string token);

        //// METHODEN FUER ACC VERWALTUNG

        //public Task GenerateEmailConfirmationTokenAsync(IdentityUser user);

        ////public async Task GenerateForgotPasswordTokenAsync(ApplicationUser user)

        ////public async Task<SignInResult> PasswordSignInAsync(SignInModel signInModel)

        ////public async Task SignOutAsync()

        ////public async Task<IdentityResult> ChangePasswordAsync(ChangePasswordModel model)

        //public Task<IdentityUser> GetUserByEmailAsync(string email);


        //public Task<IdentityResult> ConfirmEmailAsync(string uid, string token);

        ////public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordModel model)

        //public Task SendForgotPasswordEmail(IdentityUser user, string token);


        Task<Account> GetUserByEmailAsync(string email);

        Task<IdentityResult> CreateUserAsync(SignUpUserModel userModel);

        Task<SignInResult> PasswordSignInAsync(SignInModel signInModel);

        Task SignOutAsync();

        Task<IdentityResult> ChangePasswordAsync(ChangePasswordModel model);

        Task<IdentityResult> ConfirmEmailAsync(string uid, string token);

        Task GenerateEmailConfirmationTokenAsync(Account user);

        Task GenerateForgotPasswordTokenAsync(Account user);

        Task<IdentityResult> ResetPasswordAsync(ResetPasswordModel model);
    }
}

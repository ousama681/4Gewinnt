using VierGewinnt.Models;
using VierGewinnt.Repositories.Interfaces;
using VierGewinnt.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace VierGewinnt.Repositories
{
    public class AccountRepository : IAccountRepository
    {

        private readonly AppDbContext context;
        private readonly UserManager<Account> _userManager;
        private readonly SignInManager<Account> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AccountRepository(UserManager<Account> userManager,
            SignInManager<Account> signInManager,
            RoleManager<IdentityRole> roleManager,
            IUserService userService,
            IEmailService emailService,
            IConfiguration configuration,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _userService = userService;
            _emailService = emailService;
            _configuration = configuration;
            this.context = context;
        }

        public async Task<Account> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<IdentityResult> CreateUserAsync(SignUpUserModel userModel)
        {
            var user = new Account()
            {
                FirstName = userModel.FirstName,
                LastName = userModel.LastName,
                Email = userModel.Email,
                UserName = userModel.Email

            };
            var result = await _userManager.CreateAsync(user, userModel.Password);
            if (result.Succeeded)
            {
                await GenerateEmailConfirmationTokenAsync(user);
            }
            return result;
        }

        public async Task GenerateEmailConfirmationTokenAsync(Account user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            if (!string.IsNullOrEmpty(token))
            {
                await SendEmailConfirmationEmail(user, token);
            }
        }

        public async Task GenerateForgotPasswordTokenAsync(Account user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            if (!string.IsNullOrEmpty(token))
            {
                await SendForgotPasswordEmail(user, token);
            }
        }

        public async Task<SignInResult> PasswordSignInAsync(SignInModel signInModel)
        {
            return await _signInManager.PasswordSignInAsync(signInModel.Email, signInModel.Password, signInModel.RememberMe, true);
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> ChangePasswordAsync(ChangePasswordModel model)
        {
            var userId = _userService.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);
            return await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        }


        public async Task<IdentityResult> ConfirmEmailAsync(string uid, string token)
        {
            return await _userManager.ConfirmEmailAsync(await _userManager.FindByIdAsync(uid), token);
        }

        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordModel model)
        {
            return await _userManager.ResetPasswordAsync(await _userManager.FindByIdAsync(model.UserId), model.Token, model.NewPassword);
        }

        private async Task SendEmailConfirmationEmail(Account user, string token)
        {
            string appDomain = _configuration.GetSection("Application:AppDomain").Value;
            string confirmationLink = _configuration.GetSection("Application:EmailConfirmation").Value;

            UserEmailOptions options = new UserEmailOptions
            {
                ToEmails = new List<string>() { user.Email },
                PlaceHolders = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("{{UserName}}", user.FirstName),
                    new KeyValuePair<string, string>("{{Link}}",
                        string.Format(appDomain + confirmationLink, user.Id, token))
                }
            };

            await _emailService.SendEmailForEmailConfirmation(options);
        }

        private async Task SendForgotPasswordEmail(Account user, string token)
        {
            string appDomain = _configuration.GetSection("Application:AppDomain").Value;
            string confirmationLink = _configuration.GetSection("Application:ForgotPassword").Value;

            UserEmailOptions options = new UserEmailOptions
            {
                ToEmails = new List<string>() { user.Email },
                PlaceHolders = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("{{UserName}}", user.FirstName),
                    new KeyValuePair<string, string>("{{Link}}",
                        string.Format(appDomain + confirmationLink, user.Id, token))
                }
            };

            await _emailService.SendEmailForForgotPassword(options);
        }

        public async Task<bool> AddAsync(Account item)
        {
            context.Accounts.Add(item);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Account item)
        {
            Account accountFound = await context.Accounts.FindAsync(item.Id) == null ? new Account() { Id = "-1" } : await context.Accounts.FindAsync(item.Id);
            //Account accountFound = await context.Accounts.FindAsync(item.Id);

            //if (accountFound == null)
            if (accountFound.Id.Equals("-1"))
            {
                return false;
            }

            context.Accounts.Remove(item);
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<List<Account>> GetAllAsync()
        {
            return await context.Accounts.ToListAsync();
        }

        public async Task<Account> GetByIdAsync(Account item)
        {
            Account accountFound = await context.Accounts.FindAsync(item.Id) == null ? new Account() { Id = "-1" } : await context.Accounts.FindAsync(item.Id);
            //Account accountFound = await context.Accounts.FindAsync(item.Id);

            //if (accountFound == null)

            return accountFound;
        }

        public async Task UpdateAsync(Account item)
        {
            throw new NotImplementedException();
        }
    }

















    //private readonly AppDbContext context;
    //private readonly IEmailService emailService;
    //private readonly IUserService userService;
    //private readonly IConfiguration configuration;


    //private readonly UserManager<IdentityUser> userManager;
    //private readonly SignInManager<IdentityUser> signInManager;

    //public AccountRepository(AppDbContext context, IEmailService emailService, IUserService userService,
    //    IConfiguration configuration, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    //{
    //    this.context = context;
    //    this.emailService = emailService;
    //    this.userService = userService;
    //    this.configuration = configuration;
    //    this.userManager = userManager;
    //    this.signInManager = signInManager;
    //}

    //public async Task<bool> AddAsync(Account item)
    //{
    //    context.Accounts.Add(item);
    //    await context.SaveChangesAsync();
    //    return true;
    //}

    //public async Task<IdentityResult> ConfirmEmailAsync(string uid, string token)
    //{
    //    return await userManager.ConfirmEmailAsync(await userManager.FindByIdAsync(uid), token);
    //}

    //public async Task<bool> DeleteAsync(Account item)
    //{
    //    Account accountFound = await context.Accounts.FindAsync(item.Id) == null ? new Account() { Id = -1 } : await context.Accounts.FindAsync(item.Id);
    //    //Account accountFound = await context.Accounts.FindAsync(item.Id);

    //    //if (accountFound == null)
    //    if (accountFound.Id == -1)
    //    {
    //        return false;
    //    }

    //    context.Accounts.Remove(item);
    //    await context.SaveChangesAsync();

    //    return true;
    //}

    //public async Task GenerateEmailConfirmationTokenAsync(IdentityUser user)
    //{
    //    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
    //    if (!string.IsNullOrEmpty(token))
    //    {
    //        await SendEmailConfirmationEmail(user, token);
    //    }
    //}

    //public async Task<List<Account>> GetAllAsync()
    //{
    //    return await context.Accounts.ToListAsync();
    //}

    //public Task<Account> GetByEmailAsync(Account item)
    //{
    //    throw new NotImplementedException();
    //}

    //public Task<Account> GetByIdAsync(Account item)
    //{
    //    throw new NotImplementedException();
    //}

    //public async Task<IdentityUser> GetUserByEmailAsync(string email)
    //{
    //    throw new NotImplementedException();
    //}

    //public async Task SendEmailConfirmationEmail(IdentityUser user, string token)
    //{
    //    string appDomain = configuration.GetSection("Application:AppDomain").Value;
    //    string confirmationLink = configuration.GetSection("Application:EmailConfirmation").Value;

    //    UserEmailOptions options = new UserEmailOptions
    //    {
    //        ToEmails = new List<string>() { user.Email },
    //        PlaceHolders = new List<KeyValuePair<string, string>>()
    //            {
    //                new KeyValuePair<string, string>("{{UserName}}", user.Email.Split("@")[0]),
    //                new KeyValuePair<string, string>("{{Link}}",
    //                    string.Format(appDomain + confirmationLink, user.Id, token))
    //            }
    //    };

    //    await emailService.SendEmailForEmailConfirmation(options);
    //}

    //public Task SendForgotPasswordEmail(IdentityUser user, string token)
    //{
    //    throw new NotImplementedException();
    //}

    //Task IRepository<Account>.UpdateAsync(Account item)
    //{
    //    throw new NotImplementedException();
    //}

    ////public Task UpdateAsync(Account item)
    ////{
    ////    throw new NotImplementedException();
    ////}
    //////public AccountRepository(AppDbContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    //////{
    //////    this.context = context;
    //////    this.userManager = userManager;
    //////    this.signInManager = signInManager;
    //////}

    ////public async Task<bool> AddAsync(Account item)
    ////{
    ////    context.Accounts.Add(item);
    ////    await context.SaveChangesAsync();
    ////    return true;
    ////}

    ////public async Task<bool> DeleteAsync(Account item)
    ////{
    ////    Account accountFound = await context.Accounts.FindAsync(item.Id) == null ? new Account() { Id = -1 } : await context.Accounts.FindAsync(item.Id);
    ////    //Account accountFound = await context.Accounts.FindAsync(item.Id);

    ////    //if (accountFound == null)
    ////    if (accountFound.Id == -1)
    ////    {
    ////        return false;
    ////    }

    ////    context.Accounts.Remove(item);
    ////    await context.SaveChangesAsync();

    ////    return true;
    ////}

    ////public async Task<List<Account>> GetAllAsync()
    ////{
    ////    return await context.Accounts.ToListAsync();
    ////}

    ////public async Task<Account> GetByIdAsync(Account item)
    ////{
    ////    Account accountFound = await context.Accounts.FindAsync(item.Id) == null ? new Account() { Id = -1 } : await context.Accounts.FindAsync(item.Id);
    ////    //Account accountFound = await context.Accounts.FindAsync(item.Id);

    ////    //if (accountFound == null)

    ////    return accountFound;
    ////}

    ////public async Task<Account> GetByEmailAsync(Account item)
    ////{
    ////    throw new NotImplementedException();
    ////}

    ////public async Task UpdateAsync(Account item)
    ////{
    ////    throw new NotImplementedException();
    ////}





    //// METHODEN FUER ACC VERWALTUNG


    //public async Task GenerateEmailConfirmationTokenAsync(IdentityUser user)
    //{
    //    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
    //    if (!string.IsNullOrEmpty(token))
    //    {
    //        await SendEmailConfirmationEmail(user, token);
    //    }
    //}

    //private async Task SendEmailConfirmationEmail(IdentityUser user, string token)
    //{
    //    string appDomain = configuration.GetSection("Application:AppDomain").Value;
    //    string confirmationLink = configuration.GetSection("Application:EmailConfirmation").Value;

    //    UserEmailOptions options = new UserEmailOptions
    //    {
    //        ToEmails = new List<string>() { user.Email },
    //        PlaceHolders = new List<KeyValuePair<string, string>>()
    //        {
    //            new KeyValuePair<string, string>("{{UserName}}", user.Email.Split("@")[0]),
    //            new KeyValuePair<string, string>("{{Link}}",
    //                string.Format(appDomain + confirmationLink, user.Id, token))
    //        }
    //    };

    //    await emailService.SendEmailForEmailConfirmation(options);
    //}

    ////public async Task GenerateForgotPasswordTokenAsync(ApplicationUser user)
    ////{
    ////    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
    ////    if (!string.IsNullOrEmpty(token))
    ////    {
    ////        await SendForgotPasswordEmail(user, token);
    ////    }
    ////}

    ////public async Task<SignInResult> PasswordSignInAsync(SignInModel signInModel)
    ////{
    ////    return await _signInManager.PasswordSignInAsync(signInModel.Email, signInModel.Password, signInModel.RememberMe, true);
    ////}

    ////public async Task SignOutAsync()
    ////{
    ////    await _signInManager.SignOutAsync();
    ////}

    ////public async Task<IdentityResult> ChangePasswordAsync(ChangePasswordModel model)
    ////{
    ////    var userId = _userService.GetUserId();
    ////    var user = await _userManager.FindByIdAsync(userId);
    ////    return await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
    ////}

    //public async Task<IdentityUser> GetUserByEmailAsync(string email)
    //{
    //    return await userManager.FindByEmailAsync(email);
    //}


    //public async Task<IdentityResult> ConfirmEmailAsync(string uid, string token)
    //{
    //    return await userManager.ConfirmEmailAsync(await userManager.FindByIdAsync(uid), token);
    //}

    ////public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordModel model)
    ////{
    ////    return await _userManager.ResetPasswordAsync(await _userManager.FindByIdAsync(model.UserId), model.Token, model.NewPassword);
    ////}

    ////private async Task SendForgotPasswordEmail(IdentityUser user, string token)
    ////{
    ////    string appDomain = configuration.GetSection("Application:AppDomain").Value;
    ////    string confirmationLink = configuration.GetSection("Application:ForgotPassword").Value;

    ////    UserEmailOptions options = new UserEmailOptions
    ////    {
    ////        ToEmails = new List<string>() { user.Email },
    ////        PlaceHolders = new List<KeyValuePair<string, string>>()
    ////        {
    ////            new KeyValuePair<string, string>("{{UserName}}", user.Email),
    ////            new KeyValuePair<string, string>("{{Link}}",
    ////                string.Format(appDomain + confirmationLink, user.Id, token))
    ////        }
    ////    };

    ////    await emailService.SendEmailForForgotPassword(options);
    ////}

    ////async Task SendEmailConfirmationEmail(IdentityUser user, string token)
    ////{
    ////    string appDomain = configuration.GetSection("Application:AppDomain").Value;
    ////    string confirmationLink = configuration.GetSection("Application:EmailConfirmation").Value;

    ////    UserEmailOptions options = new UserEmailOptions
    ////    {
    ////        ToEmails = new List<string>() { user.Email },
    ////        PlaceHolders = new List<KeyValuePair<string, string>>()
    ////            {
    ////                new KeyValuePair<string, string>("{{UserName}}", user.Email.Split("@")[0]),
    ////                new KeyValuePair<string, string>("{{Link}}",
    ////                    string.Format(appDomain + confirmationLink, user.Id, token))
    ////            }
    ////    };

    ////    await emailService.SendEmailForEmailConfirmation(options);
    ////}

    //Task SendForgotPasswordEmail(IdentityUser user, string token)
    //{
    //    throw new NotImplementedException();
    //}
}


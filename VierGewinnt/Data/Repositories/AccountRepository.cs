using VierGewinnt.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VierGewinnt.Data.Interfaces;
using VierGewinnt.Models;
using VierGewinnt.Data.Models;

namespace VierGewinnt.Data.Repositories
{
    public class AccountRepository : IAccountRepository
    {

        private readonly AppDbContext context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AccountRepository(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IUserService userService,
            IEmailService emailService,
            IConfiguration configuration,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userService = userService;
            _emailService = emailService;
            _configuration = configuration;
            this.context = context;
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<IdentityResult> CreateUserAsync(SignUpUserModel userModel)
        {
            string pw = userModel.Password;
            var user = new ApplicationUser()
            {
                Email = userModel.Email,
                UserName = userModel.Email,
            };
            var result = await _userManager.CreateAsync(user, pw);
            if (result.Succeeded)
            {
                await GenerateEmailConfirmationTokenAsync(user);
            }
            return result;
        }

        public async Task GenerateEmailConfirmationTokenAsync(ApplicationUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            if (!string.IsNullOrEmpty(token))
            {
                await SendEmailConfirmationEmail(user, token);
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

        private async Task SendEmailConfirmationEmail(ApplicationUser user, string token)
        {
            string appDomain = _configuration.GetSection("Application:AppDomain").Value;
            string confirmationLink = _configuration.GetSection("Application:EmailConfirmation").Value;

            UserEmailOptions options = new UserEmailOptions
            {
                ToEmails = new List<string>() { user.Email },
                PlaceHolders = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("{{UserName}}", user.UserName),
                    new KeyValuePair<string, string>("{{Link}}",
                        string.Format(appDomain + confirmationLink, user.Id, token))
                }
            };

            await _emailService.SendEmailForEmailConfirmation(options);
        }

        public async Task<bool> AddAsync(ApplicationUser item)
        {
            context.Accounts.Add(item);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(ApplicationUser item)
        {
            ApplicationUser accountFound = await context.Accounts.FindAsync(item.Id) == null ? new ApplicationUser() { Id = "-1" } : await context.Accounts.FindAsync(item.Id);
            if (accountFound.Id.Equals("-1"))
            {
                return false;
            }

            context.Accounts.Remove(item);
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<List<ApplicationUser>> GetAllAsync()
        {
            return await context.Accounts.ToListAsync();
        }

        public async Task<ApplicationUser> GetByIdAsync(ApplicationUser item)
        {
            ApplicationUser accountFound = await context.Accounts.FindAsync(item.Id) == null ? new ApplicationUser() { Id = "-1" } : await context.Accounts.FindAsync(item.Id);
            return accountFound;
        }

        public async Task UpdateAsync(ApplicationUser item)
        {
            throw new NotImplementedException();
        }
    }
}


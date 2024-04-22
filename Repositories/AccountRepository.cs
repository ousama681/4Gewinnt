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
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AccountRepository(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
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

        public async Task<IdentityUser> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<IdentityResult> CreateUserAsync(SignUpUserModel userModel)
        {
            string pw = userModel.Password;
            var user = new IdentityUser()
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

        public async Task GenerateEmailConfirmationTokenAsync(IdentityUser user)
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


        public async Task<IdentityResult> ConfirmEmailAsync(string email, string token)
        {
            return await _userManager.ConfirmEmailAsync(await _userManager.FindByIdAsync(email), token);
        }

        private async Task SendEmailConfirmationEmail(IdentityUser user, string token)
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
                        string.Format(appDomain + confirmationLink, user.Email, token))
                }
            };

            await _emailService.SendEmailForEmailConfirmation(options);
        }

        public async Task<bool> AddAsync(IdentityUser item)
        {
            context.Accounts.Add(item);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(IdentityUser item)
        {
            IdentityUser accountFound = await context.Accounts.FindAsync(item.Id) == null ? new IdentityUser() { Id = "-1" } : await context.Accounts.FindAsync(item.Id);
            if (accountFound.Id.Equals("-1"))
            {
                return false;
            }

            context.Accounts.Remove(item);
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<List<IdentityUser>> GetAllAsync()
        {
            return await context.Accounts.ToListAsync();
        }

        public async Task<IdentityUser> GetByIdAsync(IdentityUser item)
        {
            IdentityUser accountFound = (await context.Accounts.FindAsync(item.Id) == null ? new IdentityUser() { Id = "-1" } : await context.Accounts.FindAsync(item.Id));
            return accountFound;
        }

        public async Task UpdateAsync(IdentityUser item)
        {
            throw new NotImplementedException();
        }
    }
}


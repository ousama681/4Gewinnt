//using VierGewinnt.Repositories;
//using VierGewinnt.Repositories;
//using VierGewinnt.Repositories.Interfaces;
//using VierGewinnt.ViewModels;
using VierGewinnt.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
//using NETCore.MailKit.Core;
using System.Net;
using System.Net.Mail;
using VierGewinnt.Models;

namespace VierGewinnt.Controllers
{
    //[Authorize]
    public class AccountController : Controller
    {
        //private readonly UserManager<ApplicationUser> userManager;
        //private readonly SignInManager<ApplicationUser> signInManager;
        //private readonly ILogger<AccountController> logger;

        //public AccountController(UserManager<ApplicationUser> userManager,
        //    SignInManager<ApplicationUser> signInManager,
        //    ILogger<AccountController> logger)
        //{
        //    this.userManager = userManager;
        //    this.signInManager = signInManager;
        //    this.logger = logger;
        //}

        //[HttpPost]
        //public async Task<IActionResult> Logout()
        //{
        //    await signInManager.SignOutAsync();
        //    return RedirectToAction("index", "home");
        //}

        //private readonly UserManager<IdentityUser> userManager;
        //private readonly SignInManager<IdentityUser> signInManager;
        //private readonly IAccountRepository accountRepository;

        //public AccountController(UserManager<IdentityUser> userManager,
        //    SignInManager<IdentityUser> signInManager,
        //    IAccountRepository accountRepository)
        //{
        //    this.userManager = userManager;
        //    this.signInManager = signInManager;
        //    this.accountRepository = accountRepository;
        //}

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> Logout()
        //{
        //    await signInManager.SignOutAsync();
        //    return RedirectToAction("index", "home");
        //}

        //[HttpGet]
        //public IActionResult Login()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public async Task<IActionResult> Login(LoginViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var result = await signInManager.PasswordSignInAsync(
        //            model.Email, model.Password, model.RememberMe, false);

        //        if (result.Succeeded)
        //        {
        //            return RedirectToAction("index", "home");
        //        }

        //        ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
        //    }

        //    return View(model);
        //}




        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult>Register(string email, string password)
        //{
        //    return View();
        //}

        //public async Task<IActionResult> Register([Bind("E-Mail,Password")] Account account



        //[HttpGet]
        //public IActionResult Register()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public async Task<IActionResult> Register(RegisterViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        // Copy data from RegisterViewModel to IdentityUser
        //        var user = new IdentityUser
        //        {
        //            UserName = model.Email,
        //            Email = model.Email
        //        };

        //        // Store user data in AspNetUsers database table
        //        var result = await userManager.CreateAsync(user, model.Password);

        //        // If user is successfully created, sign-in the user using
        //        // SignInManager and redirect to index action of HomeController
        //        if (result.Succeeded)
        //        {
        //            await signInManager.SignInAsync(user, isPersistent: false);
        //            return RedirectToAction("index", "home");
        //        }

        //        // If there are any errors, add them to the ModelState object
        //        // which will be displayed by the validation summary tag helper
        //        foreach (var error in result.Errors)
        //        {
        //            ModelState.AddModelError(string.Empty, error.Description);
        //        }
        //    }

        //    return View(model);
        //}

        //[HttpPost]
        //[AllowAnonymous]
        //public async Task<IActionResult> Register(RegisterViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = new IdentityUser
        //        {
        //            UserName = model.Email,
        //            Email = model.Email
        //        };

        //        var result = await userManager.CreateAsync(user, model.Password);

        //        if (result.Succeeded)
        //        {
        //            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        //            if (!string.IsNullOrEmpty(token))
        //            {

        //            }

        //            var confirmationLink = Url.Action("ConfirmEmail", "Account",
        //                new { userId = user.Id, Token = token }, Request.Scheme);

        //            //logger.Log(LogLevel.Warning, confirmationLink);

        //            if (signInManager.IsSignedIn(User) && User.IsInRole("Admin"))
        //            {
        //                return RedirectToAction("ListUsers", "Administration");
        //            }

        //            ViewBag.ErrorTitle = "Registration successful";
        //            ViewBag.ErrorMessage = "Before you can Login, please confirm your " +
        //                    "email, by clicking on the confirmation link we have emailed you";
        //            return View(model);
        //        }

        //        foreach (var error in result.Errors)
        //        {
        //            ModelState.AddModelError(string.Empty, error.Description);
        //        }
        //    }

        //    return View(model);
        //}

        //[AllowAnonymous]
        //public async Task<IActionResult> ConfirmEmail(string userId, string token)
        //{
        //    if (userId == null || token == null)
        //    {
        //        return RedirectToAction("index", "home");
        //    }

        //    var user = await userManager.FindByIdAsync(userId);
        //    if (user == null)
        //    {
        //        ViewBag.ErrorMessage = $"The User ID {userId} is invalid";
        //        return View("NotFound");
        //    }

        //    var result = await userManager.ConfirmEmailAsync(user, token);
        //    if (result.Succeeded)
        //    {
        //        return View();
        //    }

        //    ViewBag.ErrorTitle = "Email cannot be confirmed";
        //    return View("Error");
        //}

        // rest of the code






        //public async Task<IActionResult> Email()
        //{
        //    return View();
        //}

        //public async Task<IActionResult> ChangePassword()
        //{
        //    return View();
        //}

        //private async Task SendEmailConfirmationEmail(IdentityUser user, string token)
        //{
        //    string appDomain = configuration.GetSection("Application:AppDomain").Value;
        //    string confirmationLink = configuration.GetSection("Application:EmailConfirmation").Value;

        //    UserEmailOptions options = new UserEmailOptions()
        //    {
        //        ToEmails = new List<string>() { user.Email },
        //        PlaceHolders = new List<KeyValuePair<string, string>>()
        //        {
        //            new KeyValuePair<string, string>("{{UserName}}", "John")
        //        }
        //    };
        //}
        //}

        //[HttpGet("confirm-email")]
        //    public async Task<IActionResult> ConfirmEmail(string uid, string token, string email)
        //    {
        //        EmailConfirmModel model = new EmailConfirmModel
        //        {
        //            Email = email
        //        };

        //        if (!string.IsNullOrEmpty(uid) && !string.IsNullOrEmpty(token))
        //        {
        //            token = token.Replace(' ', '+');
        //            var result = await accountRepository.ConfirmEmailAsync(uid, token);
        //            if (result.Succeeded)
        //            {
        //                model.EmailVerified = true;
        //            }
        //        }

        //        return View(model);
        //    }

        //    [HttpPost("confirm-email")]
        //    public async Task<IActionResult> ConfirmEmail(EmailConfirmModel model)
        //    {
        //        var user = await accountRepository.GetUserByEmailAsync(model.Email);
        //        if (user != null)
        //        {
        //            if (user.EmailConfirmed)
        //            {
        //                model.EmailVerified = true;
        //                return View(model);
        //            }

        //            await accountRepository.GenerateEmailConfirmationTokenAsync(user);
        //            model.EmailSent = true;
        //            ModelState.Clear();
        //        }
        //        else
        //        {
        //            ModelState.AddModelError("", "Something went wrong.");
        //        }
        //        return View(model);
        //    }
        //}












        private readonly IAccountRepository _accountRepository;

        public AccountController(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        [Route("account/signin")]
        public IActionResult SignIn()
        {
            return View();
        }

        [Route("account/signup")]
        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpUserModel userModel)
        {
            if (ModelState.IsValid)
            {
                // write your code
                var result = await _accountRepository.CreateUserAsync(userModel);
                if (!result.Succeeded)
                {
                    foreach (var errorMessage in result.Errors)
                    {
                        ModelState.AddModelError("", errorMessage.Description);
                    }

                    return View(userModel);
                }

                ModelState.Clear();
                return RedirectToAction("ConfirmEmailTest", new { email = userModel.Email });
            }

            return View(userModel);
        }


        //[Route("login")]
        public IActionResult Login()
        {
            return View();
        }

        //[Route("login")]
        [HttpPost]
        public async Task<IActionResult> SignIn(SignInModel signInModel)
        {
            //returnUrl = "localhost:5155/Account/Login";
            if (ModelState.IsValid)
            {
                var result = await _accountRepository.PasswordSignInAsync(signInModel);
                if (result.Succeeded)
                {
                    //if (!string.IsNullOrEmpty(returnUrl))
                    //{
                    //    return LocalRedirect(returnUrl);
                    //}
                    return RedirectToAction("Index", "Home");
                }
                if (result.IsNotAllowed)
                {
                    ModelState.AddModelError("", "Not allowed to login");
                }
                else if (result.IsLockedOut)
                {
                    ModelState.AddModelError("", "Account blocked. Try after some time.");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid credentials");
                }

            }

            return View(signInModel);
        }

        //[Route("logout")]
        public async Task<IActionResult> Logout()
        {
            await _accountRepository.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Route("change-password")]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _accountRepository.ChangePasswordAsync(model);
                if (result.Succeeded)
                {
                    ViewBag.IsSuccess = true;
                    ModelState.Clear();
                    return View();
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

            }
            return View(model);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string uid, string token, string email)
        {
            EmailConfirmModel model = new EmailConfirmModel
            {
                Email = email
            };

            if (!string.IsNullOrEmpty(uid) && !string.IsNullOrEmpty(token))
            {
                token = token.Replace(' ', '+');
                var result = await _accountRepository.ConfirmEmailAsync(uid, token);
                if (result.Succeeded)
                {
                    model.EmailVerified = true;
                }
            }

            return View(model);
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmailTest(EmailConfirmModel model)
        {
            var user = await _accountRepository.GetUserByEmailAsync(model.Email);
            if (user != null)
            {
                if (user.EmailConfirmed)
                {
                    model.EmailVerified = true;
                    return View(model);
                }

                await _accountRepository.GenerateEmailConfirmationTokenAsync(user);
                model.EmailSent = true;
                ModelState.Clear();
            }
            else
            {
                ModelState.AddModelError("", "Something went wrong.");
            }
            return View(model);
        }

        [AllowAnonymous, HttpGet("forgot-password")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [AllowAnonymous, HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                // code here
                var user = await _accountRepository.GetUserByEmailAsync(model.Email);
                if (user != null)
                {
                    await _accountRepository.GenerateForgotPasswordTokenAsync(user);
                }

                ModelState.Clear();
                model.EmailSent = true;
            }
            return View(model);
        }

        [AllowAnonymous, HttpGet("reset-password")]
        public IActionResult ResetPassword(string uid, string token)
        {
            ResetPasswordModel resetPasswordModel = new ResetPasswordModel
            {
                Token = token,
                UserId = uid
            };
            return View(resetPasswordModel);
        }

        [AllowAnonymous, HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                model.Token = model.Token.Replace(' ', '+');
                var result = await _accountRepository.ResetPasswordAsync(model);
                if (result.Succeeded)
                {
                    ModelState.Clear();
                    model.IsSuccess = true;
                    return View(model);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }
    }

}
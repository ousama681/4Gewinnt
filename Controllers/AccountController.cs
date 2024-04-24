using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VierGewinnt.Models;
using VierGewinnt.Repositories.Interfaces;

namespace VierGewinnt.Controllers
{
    public class AccountController : Controller
    {

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

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

        [Route("account/signin")]
        [HttpPost]
        public async Task<IActionResult> SignIn(SignInModel signInModel)
        {
            if (ModelState.IsValid)
            {
                var result = await _accountRepository.PasswordSignInAsync(signInModel);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                if (result.IsNotAllowed)
                {
                    ModelState.AddModelError("", "Not allowed to login");
                }
                else if (result.IsLockedOut)
                {
                    ModelState.AddModelError("", "User blocked. Try after some time.");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid credentials");
                }

            }



            return View(new SignInUpModel() { SignInModel = signInModel });
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

                    return RedirectToAction("SignIn");
                }

                ModelState.Clear();
                //return RedirectToAction("ConfirmEmail", new { email = userModel.Email });
                return RedirectToAction("ConfirmEmail", new EmailConfirmModel() { Email = userModel.Email, IsConfirmed = false, EmailSent=true});
            }

            return RedirectToAction("SignIn", new SignInUpModel() { SignUpModel = userModel });
        }

        public IActionResult Login()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _accountRepository.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
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

        [Route("account/confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string uid, string token)
        {
            EmailConfirmModel model = new EmailConfirmModel() {Uid = uid, Token = token };
            if (!string.IsNullOrEmpty(model.Uid) && !string.IsNullOrEmpty(model.Token))
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
    }
}
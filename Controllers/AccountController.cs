using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using DentalHealthTracker.Models;
using DentalHealthTracker.Services;
using Microsoft.Extensions.Logging;

namespace DentalHealthTracker.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly IPasswordResetService _passwordResetService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IUserService userService,
            IEmailService emailService,
            IPasswordResetService passwordResetService,
            ILogger<AccountController> logger)
        {
            _userService = userService;
            _emailService = emailService;
            _passwordResetService = passwordResetService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            _logger.LogInformation("Login POST çağrıldı. Email: {Email}", model.Email);
            if (ModelState.IsValid)
            {
                var result = await _userService.ValidateUserAsync(model.Email, model.Password);
                if (result.Succeeded)
                {
                    var user = await _userService.GetUserByEmailAsync(model.Email);
                    if (user != null)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.Email),
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                        };

                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                        TempData["Message"] = "Başarıyla giriş yaptınız.";
                        _logger.LogInformation("Giriş başarılı. Email: {Email}", model.Email);
                        return RedirectToAction("Index", "Home");
                    }
                }
                _logger.LogWarning("Giriş başarısız. Email: {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Geçersiz e-posta veya parola.");
            }
            else
            {
                _logger.LogWarning("ModelState geçersiz. Hatalar: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.RegisterAsync(model);
                if (result.Succeeded)
                {
                    await _emailService.SendWelcomeEmailAsync(model.Email, model.FullName);
                    TempData["Message"] = "Kayıt işlemi başarıyla tamamlandı. Giriş yapabilirsiniz.";
                    return RedirectToAction("Login");
                }
                ModelState.AddModelError(string.Empty, "Kayıt işlemi başarısız oldu.");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Message"] = "Başarıyla çıkış yaptınız.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var user = _userService.GetUserByEmailAsync(User.Identity?.Name ?? string.Empty).Result;
            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                Email = user.Email,
                FullName = user.FullName,
                BirthDate = user.BirthDate
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Profile", model);
            }

            var result = await _userService.UpdateProfileAsync(User.Identity?.Name ?? string.Empty, model);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Profile));
            }

            TempData["SuccessMessage"] = "Profil bilgileriniz başarıyla güncellendi.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _userService.ChangePasswordAsync(User.Identity?.Name ?? string.Empty, model);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Message;
                return View(model);
            }

            TempData["SuccessMessage"] = "Şifreniz başarıyla güncellendi.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _passwordResetService.CreatePasswordResetTokenAsync(model.Email);
                if (result.Succeeded)
                {
                    TempData["Message"] = "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi.";
                    return RedirectToAction(nameof(Login));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Geçersiz şifre sıfırlama bağlantısı.";
                return RedirectToAction("Error", "Home");
            }

            var validationResult = await _passwordResetService.ValidateTokenAsync(token);
            if (!validationResult.Succeeded)
            {
                TempData["ErrorMessage"] = validationResult.Message;
                return RedirectToAction("Error", "Home");
            }

            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Lütfen tüm alanları doğru şekilde doldurun.";
                return View(model);
            }

            var result = await _passwordResetService.ResetPasswordAsync(model.Token, model.NewPassword);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Message;
                return View(model);
            }

            TempData["SuccessMessage"] = "Şifreniz başarıyla sıfırlandı. Yeni şifrenizle giriş yapabilirsiniz.";
            return RedirectToAction("ResetPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}
using car_rental_system.Data;
using car_rental_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace car_rental_system.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>Customer login — Admin accounts must use <see cref="AdminLogin"/>.</summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToLocal(returnUrl);

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains(IdentitySeed.AdminRole))
                {
                    ModelState.AddModelError(string.Empty,
                        "Admin account ke liye alag Admin Login page use karein (navbar mein \"Admin login\").");
                    return View(model);
                }

                await _signInManager.SignInAsync(user, model.RememberMe);
                await _signInManager.RefreshSignInAsync(user);
                return RedirectAfterCustomerSignIn(model.ReturnUrl);
            }

            if (result.IsLockedOut)
                ModelState.AddModelError(string.Empty, "Account is locked out. Try again later.");
            else
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");

            return View(model);
        }

        /// <summary>Admin-only login.</summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AdminLogin(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectIfAlreadySignedIn(returnUrl);

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminLogin(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains(IdentitySeed.AdminRole))
                {
                    ModelState.AddModelError(string.Empty,
                        "Ye admin account nahi hai. Customers ke liye \"Customer login\" use karein.");
                    return View(model);
                }

                await _signInManager.SignInAsync(user, model.RememberMe);
                await _signInManager.RefreshSignInAsync(user);
                return RedirectAfterAdminSignIn(model.ReturnUrl);
            }

            if (result.IsLockedOut)
                ModelState.AddModelError(string.Empty, "Account is locked out. Try again later.");
            else
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Car");

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new IdentityUser { UserName = model.Email, Email = model.Email, EmailConfirmed = true };
            var create = await _userManager.CreateAsync(user, model.Password);
            if (!create.Succeeded)
            {
                foreach (var err in create.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, IdentitySeed.UserRole);
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Car");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Car");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied() => View();

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            if (User.IsInRole(IdentitySeed.AdminRole))
                return RedirectToAction("Index", "Admin");

            return RedirectToAction("Index", "Car");
        }

        private IActionResult RedirectIfAlreadySignedIn(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            if (User.IsInRole(IdentitySeed.AdminRole))
                return RedirectToAction("Index", "Admin");

            return RedirectToAction("Index", "Car");
        }

        private IActionResult RedirectAfterCustomerSignIn(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Car");
        }

        private IActionResult RedirectAfterAdminSignIn(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Admin");
        }
    }
}

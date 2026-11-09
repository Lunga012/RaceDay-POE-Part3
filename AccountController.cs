using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers
{
    public class AccountController : Controller
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IApiClient apiClient, ILogger<AccountController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // If already logged in, redirect to respective dashboard
            var role = HttpContext.Session.GetString("UserRole");
            if (!string.IsNullOrWhiteSpace(role))
            {
                return RedirectBasedOnRole(role);
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, data, error) = await _apiClient.LoginAsync(model);
            if (!success || data == null)
            {
                ModelState.AddModelError(string.Empty, error ?? "Invalid email or password.");
                return View(model);
            }

            // Establish MVC session
            SetUserSession(data);
            TempData["SuccessMessage"] = $"Welcome back, {data.Name}!";

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectBasedOnRole(data.RoleName);
        }

        [HttpGet]
        public IActionResult Register()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (!string.IsNullOrWhiteSpace(role))
            {
                return RedirectBasedOnRole(role);
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, data, error) = await _apiClient.RegisterAsync(model);
            if (!success || data == null)
            {
                ModelState.AddModelError(string.Empty, error ?? "Failed to create account. Please check your information.");
                return View(model);
            }

            // Establish MVC session
            SetUserSession(data);
            TempData["SuccessMessage"] = $"Welcome to RaceDay, {data.Name}! Your account has been registered as {data.RoleName}.";

            return RedirectBasedOnRole(data.RoleName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _apiClient.LogoutAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while notifying backend of logout.");
            }

            HttpContext.Session.Clear();
            TempData["InfoMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private void SetUserSession(AuthResponseDto data)
        {
            HttpContext.Session.SetString("SessionToken", data.SessionToken);
            HttpContext.Session.SetString("UserId", data.UserID.ToString());
            HttpContext.Session.SetString("UserName", data.Name);
            HttpContext.Session.SetString("UserEmail", data.Email);
            HttpContext.Session.SetString("UserRole", data.RoleName);
        }

        private IActionResult RedirectBasedOnRole(string role)
        {
            if (role.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Dashboard", "Organiser");
            }
            return RedirectToAction("MyEvents", "Participant");
        }
    }
}

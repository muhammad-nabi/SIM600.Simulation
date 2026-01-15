#nullable disable

using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using SIM600.Simulation.Constants;

namespace SIM600.Simulation.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginWithMagicLinkModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginWithMagicLinkModel> _logger;

        public LoginWithMagicLinkModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<LoginWithMagicLinkModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public string StatusMessage { get; set; }

        public string ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string code, string returnUrl = null)
        {
            // Validate returnUrl to prevent open redirect
            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
            {
                returnUrl = Url.Content("~/");
            }
            ReturnUrl = returnUrl;

            // If user is already logged in, redirect to home
            if (User.Identity?.IsAuthenticated == true)
            {
                return LocalRedirect(returnUrl);
            }

            // Validate parameters
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
            {
                StatusMessage = "Invalid sign-in link.";
                return Page();
            }

            // Find user
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Magic link login attempted with invalid user ID: {UserId}", userId);
                StatusMessage = "Invalid sign-in link.";
                return Page();
            }

            // Decode and verify token
            string decodedCode;
            try
            {
                decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            }
            catch
            {
                _logger.LogWarning("Magic link login attempted with invalid code format for user: {UserId}", userId);
                StatusMessage = "Invalid sign-in link.";
                return Page();
            }

            var isValid = await _userManager.VerifyUserTokenAsync(
                user,
                TokenOptions.DefaultProvider,
                MagicLinkConstants.TokenPurpose,
                decodedCode);

            if (!isValid)
            {
                _logger.LogWarning("Magic link login failed - invalid or expired token for user: {UserId}", userId);
                StatusMessage = "This sign-in link is invalid or has expired. Please request a new one.";
                return Page();
            }

            // Check if user email is confirmed
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                _logger.LogWarning("Magic link login attempted for unconfirmed email: {UserId}", userId);
                StatusMessage = "Please confirm your email before signing in.";
                return Page();
            }

            // Check if user is locked out
            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Magic link login attempted for locked out user: {UserId}", userId);
                StatusMessage = "Your account is locked out. Please try again later.";
                return Page();
            }

            // Check if 2FA is enabled
            if (await _userManager.GetTwoFactorEnabledAsync(user))
            {
                _logger.LogInformation("Magic link verified, redirecting to 2FA for user: {UserId}", userId);

                // Invalidate token before redirect to prevent reuse
                await _userManager.UpdateSecurityStampAsync(user);

                // Create partial sign-in for 2FA flow
                var identity = new ClaimsIdentity(IdentityConstants.TwoFactorUserIdScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, userId));
                await HttpContext.SignInAsync(
                    IdentityConstants.TwoFactorUserIdScheme,
                    new ClaimsPrincipal(identity));

                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = false });
            }

            // No 2FA - sign in directly
            await _signInManager.SignInAsync(user, isPersistent: false);

            // Invalidate token (security stamp rotation) to prevent reuse
            await _userManager.UpdateSecurityStampAsync(user);

            _logger.LogInformation("User {UserId} logged in via magic link", userId);

            return LocalRedirect(returnUrl);
        }
    }
}

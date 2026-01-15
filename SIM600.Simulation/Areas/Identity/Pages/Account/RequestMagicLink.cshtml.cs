#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using SIM600.Simulation.Constants;

namespace SIM600.Simulation.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RequestMagicLinkModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<RequestMagicLinkModel> _logger;

        public RequestMagicLinkModel(
            UserManager<IdentityUser> userManager,
            IEmailSender emailSender,
            IMemoryCache memoryCache,
            ILogger<RequestMagicLinkModel> logger)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            // Validate returnUrl to prevent open redirect
            ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // Validate returnUrl to prevent open redirect
            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
            {
                returnUrl = Url.Content("~/");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var normalizedEmail = Input.Email.ToUpperInvariant();
            var cacheKey = $"magiclink:request:{normalizedEmail}";

            // Check rate limit
            if (_memoryCache.TryGetValue(cacheKey, out int requestCount))
            {
                if (requestCount >= MagicLinkConstants.MaxRequestsPerWindow)
                {
                    ModelState.AddModelError(string.Empty,
                        $"Too many requests. Please try again in {MagicLinkConstants.RateLimitWindowMinutes} minutes.");
                    return Page();
                }
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                _logger.LogInformation("Magic link requested for non-existent or unconfirmed email: {Email}", Input.Email);
                return RedirectToPage("./RequestMagicLinkConfirmation", new { returnUrl });
            }

            // Check if user is locked out
            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Magic link requested for locked out user: {Email}", Input.Email);
                return RedirectToPage("./RequestMagicLinkConfirmation", new { returnUrl });
            }

            // Update rate limit counter
            var newCount = requestCount + 1;
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(MagicLinkConstants.RateLimitWindowMinutes));
            _memoryCache.Set(cacheKey, newCount, cacheOptions);

            // Generate magic link token
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateUserTokenAsync(
                user,
                TokenOptions.DefaultProvider,
                MagicLinkConstants.TokenPurpose);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Page(
                "/Account/LoginWithMagicLink",
                pageHandler: null,
                values: new { area = "Identity", userId, code, returnUrl },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(
                Input.Email,
                "Sign in to SIM600 Simulation",
                $"<p>Click the link below to sign in to your account:</p>" +
                $"<p><a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Sign in to SIM600 Simulation</a></p>" +
                $"<p>This link expires in {MagicLinkConstants.TokenLifespanMinutes} minutes and can only be used once.</p>" +
                $"<p>If you did not request this, please ignore this email.</p>");

            _logger.LogInformation("Magic link sent to {Email}", Input.Email);

            return RedirectToPage("./RequestMagicLinkConfirmation", new { returnUrl });
        }
    }
}

#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SIM600.Simulation.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RequestMagicLinkConfirmationModel : PageModel
    {
        public string ReturnUrl { get; set; }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }
    }
}

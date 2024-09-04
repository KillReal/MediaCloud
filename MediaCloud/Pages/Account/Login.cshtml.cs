using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.UserProvider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NLog;
using ILogger = NLog.ILogger;

namespace MediaCloud.WebApp.Pages
{
    public class LoginModel(IUserProvider userProvider) : PageModel
    {
        private readonly IUserProvider _userProvider = userProvider;
        private readonly ILogger _logger = LogManager.GetLogger("Actor");

        [BindProperty]
        public bool IsFailed { get; set; } = false;
        [BindProperty]
        public AuthData AuthData { get; set; } = new();
        [BindProperty]
        public User? CurrentUser { get; set; } = null;
        [BindProperty]
        public string ReturnUrl { get; set; } = "/";

        public IActionResult OnGet(string returnUrl = "/")
        {
            ReturnUrl = returnUrl;

            if (_userProvider.GetCurrentOrDefault() != null)
            {
                return Redirect(ReturnUrl);
            }

            return Page();
        }

        public IActionResult OnPost()
        {

            if (_userProvider.Authorize(AuthData, HttpContext) == false)
            {
                CurrentUser = null;
                _logger.Error("Failed sign attempt by name: {AuthData.Name}", AuthData.Name);
                IsFailed = true;
                
                return Page();
            }

            _logger.Info("Signed in actor with name: {AuthData.Name}", AuthData.Name);

            return Redirect(ReturnUrl);
        }
    }
}
    
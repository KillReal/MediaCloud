using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp.Services.ConfigProvider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NLog;
using ILogger = NLog.ILogger;

namespace MediaCloud.WebApp.Pages
{
    public class JoininModel(IUserProvider userProvider) : PageModel
    {
        private readonly ILogger _logger = LogManager.GetLogger("User");

        [BindProperty]
        public RegistrationResult Result { get; set; } = new RegistrationResult();
        [BindProperty]
        public string InviteCode { get; set; } = "";
        [BindProperty]
        public AuthData AuthData { get; set; } = new AuthData();
        [BindProperty]
        public User? CurrentUser { get; set; } = null;

        public IActionResult OnGet()
        {
            if (userProvider.GetCurrentOrDefault() != null)
            {
                return Redirect(Request.Headers.Referer.ToString());
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            Result = userProvider.Register(AuthData, InviteCode);

            _logger.Info(Result.Message);

            if (Result.IsSuccess)
            {
                return Redirect("/User/Login");
            }

            CurrentUser = null;
            return Page();
        }
    }
}
    
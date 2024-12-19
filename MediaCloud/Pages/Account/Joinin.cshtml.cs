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
        private readonly ILogger _logger = LogManager.GetLogger("Actor");
        private readonly IUserProvider _actorProvider = userProvider;

        [BindProperty]
        public RegistrationResult Result { get; set; } = new();
        [BindProperty]
        public string InviteCode { get; set; } = "";
        [BindProperty]
        public AuthData AuthData { get; set; } = new();
        [BindProperty]
        public User? CurrentUser { get; set; } = null;

        public IActionResult OnGet()
        {
            if (_actorProvider.GetCurrentOrDefault() != null)
            {
                return Redirect(Request.Headers.Referer.ToString());
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            Result = _actorProvider.Register(AuthData, InviteCode);

            _logger.Info(Result.Message);

            if (Result.IsSuccess)
            {
                return Redirect("/Account/Login");
            }

            CurrentUser = null;
            return Page();
        }
    }
}
    
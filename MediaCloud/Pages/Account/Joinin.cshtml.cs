using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp.Services.ConfigProvider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NLog;
using ILogger = NLog.ILogger;

namespace MediaCloud.WebApp.Pages
{
    public class JoininModel(IUserProvider actorProvider, IConfigProvider configProvider) : PageModel
    {
        private readonly ILogger _logger = LogManager.GetLogger("Actor");
        private readonly IUserProvider _actorProvider = actorProvider;
        private readonly IConfigProvider _configProvider = configProvider;

        [BindProperty]
        public RegistrationResult Result { get; set; } = new();
        [BindProperty]
        public string InviteCode { get; set; } = "";
        [BindProperty]
        public AuthData AuthData { get; set; } = new();
        [BindProperty]
        public User? CurrentActor { get; set; } = null;
        [BindProperty]
        public string ReturnUrl { get; set; } = "";

        public IActionResult OnGet(string returnUrl = "/")
        {
            ReturnUrl = returnUrl;

            if (_actorProvider.GetCurrentOrDefault() != null)
            {
                return Redirect(ReturnUrl);
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            Result = _actorProvider.Register(_configProvider, AuthData, InviteCode);

            _logger.Info(Result.Message);

            if (Result.IsSuccess)
            {
                return Redirect("/Account/Login");
            }

            CurrentActor = null;
            return Page();
        }
    }
}
    
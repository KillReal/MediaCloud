using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Pages.Actors;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.ActorProvider;
using MediaCloud.WebApp.Services.ConfigurationProvider;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NLog;
using System.Security.Claims;
using ILogger = NLog.ILogger;

namespace MediaCloud.WebApp.Pages
{
    public class JoininModel : PageModel
    {
        private readonly ILogger _logger;
        private readonly IActorProvider _actorProvider;
        private readonly IConfigProvider _configProvider;

        [BindProperty]
        public RegistrationResult Result { get; set; } = new();
        [BindProperty]
        public string InviteCode { get; set; } = "";
        [BindProperty]
        public AuthData AuthData { get; set; } = new();
        [BindProperty]
        public Actor? CurrentActor { get; set; } = null;
        [BindProperty]
        public string ReturnUrl { get; set; } = "";

        public JoininModel(IActorProvider actorProvider, IConfigProvider configProvider)
        {
            _actorProvider = actorProvider;
            _configProvider = configProvider;
            _logger = LogManager.GetLogger("Actor");
        }

        public IActionResult OnGet(string returnUrl = "/")
        {
            ReturnUrl = returnUrl;

            var actor = _actorProvider.GetCurrentOrDefault();

            if (actor != null)
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
    
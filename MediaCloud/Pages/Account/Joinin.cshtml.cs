using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Pages.Actors;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.ActorProvider;
using MediaCloud.WebApp.Services.DataService;
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

        [BindProperty]
        public RegistrationResult Result { get; set; } = new();
        [BindProperty]
        public string InviteCode { get; set; } = "";
        [BindProperty]
        public AuthData AuthData { get; set; } = new();
        [BindProperty]
        public string ReturnUrl { get; set; } = "";

        public JoininModel(IActorProvider actorProvider)
        {
            _actorProvider = actorProvider;
            _logger = LogManager.GetLogger("Actor");
        }

        public IActionResult OnGet(string returnUrl = "/")
        {
            ReturnUrl = returnUrl;

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

            return Page();
        }
    }
}
    
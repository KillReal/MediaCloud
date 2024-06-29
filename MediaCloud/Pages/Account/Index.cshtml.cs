using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.ActorProvider;
using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NLog;
using SixLabors.ImageSharp.Advanced;
using System.Security.Claims;
using IConfigProvider = MediaCloud.WebApp.Services.ConfigProvider.IConfigProvider;
using ILogger = NLog.ILogger;

namespace MediaCloud.WebApp.Pages
{
    public class IndexModel : AuthorizedPageModel
    {
        private readonly IConfigProvider _configProvider;

        [BindProperty]
        public Actor Actor { get; set; }
        [BindProperty]
        public ActorSettings ActorSettings { get; set; }
        [BindProperty]
        public EnvironmentSettings? EnvironmentSettings { get; set; }
        [BindProperty]
        public bool IsEnvironmentSettingsChanged { get; set; }

        [BindProperty]
        public string ReturnUrl { get; set; } = "/";

        public IndexModel(IActorProvider actorProvider, IConfigProvider configProvider) : base(actorProvider)
        {
            _logger = LogManager.GetLogger("Actor");
            _configProvider = configProvider;

            Actor = actorProvider.GetCurrent();

            ActorSettings = _configProvider.ActorSettings;
            EnvironmentSettings = Actor.IsAdmin 
                ? _configProvider.EnvironmentSettings 
                : null;
        }

        public IActionResult OnGet(string returnUrl = "/")
        {
            ReturnUrl = returnUrl;
            return Page();
        }

        public IActionResult OnPost()
        {
            _configProvider.ActorSettings = ActorSettings;

            var actualActor = _actorProvider.GetCurrent();

            if (IsEnvironmentSettingsChanged && EnvironmentSettings != null && actualActor.IsAdmin)
            {
                _configProvider.EnvironmentSettings = EnvironmentSettings;
            }

            return Redirect(ReturnUrl);
        }
    }
}
    
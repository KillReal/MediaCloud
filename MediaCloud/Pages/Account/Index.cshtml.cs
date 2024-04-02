using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.ActorProvider;
using MediaCloud.WebApp.Services.ConfigurationProvider;
using MediaCloud.WebApp.Services.DataService;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NLog;
using SixLabors.ImageSharp.Advanced;
using System.Security.Claims;
using IConfigProvider = MediaCloud.WebApp.Services.ConfigurationProvider.IConfigProvider;
using ILogger = NLog.ILogger;

namespace MediaCloud.WebApp.Pages
{
    public class IndexModel : AuthorizedPageModel
    {
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

        public IndexModel(IDataService dataService) : base(dataService)
        {
            _logger = LogManager.GetLogger("Actor");
            Actor = _dataService.GetCurrentActor();

            ActorSettings = dataService.ActorSettings;
            EnvironmentSettings = Actor.IsAdmin 
                ? dataService.EnvironmentSettings 
                : null;
        }

        public IActionResult OnGet(string returnUrl = "/")
        {
            ReturnUrl = returnUrl;
            return Page();
        }

        public IActionResult OnPost()
        {
            _dataService.SaveActorSettings(ActorSettings);

            if (IsEnvironmentSettingsChanged && EnvironmentSettings != null)
            {
                _dataService.SaveEnvironmentSettings(EnvironmentSettings);
            }

            return Redirect(ReturnUrl);
        }
    }
}
    
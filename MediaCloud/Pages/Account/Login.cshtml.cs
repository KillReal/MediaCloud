using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.DataService;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NLog;
using System.Security.Claims;
using ILogger = NLog.ILogger;

namespace MediaCloud.WebApp.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IDataService _dataService;
        private readonly ILogger _logger;
        private readonly IStatisticService _statisticService;

        [BindProperty]
        public bool IsFailed { get; set; } = false;
        [BindProperty]
        public AuthData AuthData { get; set; } = new();

        [BindProperty]
        public string ReturnUrl { get; set; } = "/";

        public LoginModel(IDataService dataService, IStatisticService statisticService)
        {
            _dataService = dataService;
            _logger = LogManager.GetLogger("Actor.Login");
            _statisticService = statisticService;
        }

        public IActionResult OnGet(string returnUrl = "/")
        {
            ReturnUrl = returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var actor = _dataService.Actors.GetByAuthData(AuthData);

            if (actor == null)
            {
                _logger.Error("Failed sign attempt by name: {AuthData.Name}", AuthData.Name);
                IsFailed = true;
                return Page();
            }

            var claims = new List<Claim> 
            { 
                new Claim(ClaimTypes.Name, AuthData.Name) 
            
            };
            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            _dataService.Actors.SetLastLoginAt(actor, DateTime.Now.ToUniversalTime());
            _logger.Info("Signed in actor with name: {AuthData.Name}", AuthData.Name);

            _statisticService.ActivityFactorRaised.Invoke();

            return Redirect(ReturnUrl);
        }
    }
}
    
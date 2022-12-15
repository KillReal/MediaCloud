using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MediaCloud.WebApp.Pages
{
    public class LoginModel : PageModel
    {
        private IRepository Repository;
        private ILogger _logger;

        [BindProperty]
        public bool IsFailed { get; set; } = false;
        [BindProperty]
        public AuthData AuthData { get; set; }

        [BindProperty]
        public string ReturnUrl { get; set; }

        public LoginModel(IRepository repository, ILogger<LoginModel> logger)
        {
            Repository = repository;
            _logger = logger;
        }

        public IActionResult OnGet(string returnUrl = "/")
        {
            AuthData = new();
            ReturnUrl = returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var actor = Repository.Actors.GetByAuthData(AuthData);

            if (actor == null)
            {
                _logger.LogError($"Failed sign attempt by name: {AuthData.Name}");
                IsFailed = true;
                return Page();
            }

            var claims = new List<Claim> { new Claim(ClaimTypes.Name, AuthData.Name) };
            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            Repository.Actors.SetLastLoginAt(actor, DateTime.Now);
            _logger.LogInformation($"Signed in actor with name: {AuthData.Name}");

            return Redirect(ReturnUrl);
        }
    }
}
    
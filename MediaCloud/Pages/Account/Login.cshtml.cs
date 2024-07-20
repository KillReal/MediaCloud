using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.ActorProvider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NLog;
using ILogger = NLog.ILogger;

namespace MediaCloud.WebApp.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IActorProvider _actorProvider;
        private readonly ILogger _logger;

        [BindProperty]
        public bool IsFailed { get; set; } = false;
        [BindProperty]
        public AuthData AuthData { get; set; } = new();
        [BindProperty]
        public Actor? CurrentActor { get; set; } = null;
        [BindProperty]
        public string ReturnUrl { get; set; } = "/";

        public LoginModel(IActorProvider actorProvider)
        {
            _actorProvider = actorProvider;
            _logger = LogManager.GetLogger("Actor");
        }

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

            if (_actorProvider.Authorize(AuthData, HttpContext) == false)
            {
                CurrentActor = null;
                _logger.Error("Failed sign attempt by name: {AuthData.Name}", AuthData.Name);
                IsFailed = true;
                
                return Page();
            }

            _logger.Info("Signed in actor with name: {AuthData.Name}", AuthData.Name);

            return Redirect(ReturnUrl);
        }
    }
}
    
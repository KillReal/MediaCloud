using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.UserProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NLog;
using ILogger = NLog.ILogger;

namespace MediaCloud.WebApp.Pages
{
    [Authorize]
    public class AuthorizedPageModel : PageModel
    {   
        protected IUserProvider _userProvider;
        protected ILogger _logger;

        [BindProperty]
        public User? CurrentUser { get; set; }

        public AuthorizedPageModel(IUserProvider actorProvider) 
        {
            _userProvider = actorProvider;
            _logger = LogManager.GetLogger("PageModel");

            LogPageInit();
        }

        private void LogPageInit()
        {
            CurrentUser = _userProvider.GetCurrent();

            var url = GetType().Name;

            _logger.Debug("Page: {url} initialized by {CurrentActor.Name}", url, CurrentUser.Name);
        }
    }
}

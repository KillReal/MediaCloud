using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.ActorProvider;
using MediaCloud.WebApp.Services.ConfigProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NLog;
using ILogger = NLog.ILogger;

namespace MediaCloud.WebApp.Pages
{
    [Authorize]
    public class AuthorizedPageModel : PageModel
    {   
        protected IActorProvider _actorProvider;
        protected ILogger _logger;

        [BindProperty]
        public Actor? CurrentActor { get; set; }

        public AuthorizedPageModel(IActorProvider actorProvider) 
        {
            _actorProvider = actorProvider;
            _logger = LogManager.GetLogger("PageModel");

            LogPageInit();
        }

        private void LogPageInit()
        {
            CurrentActor = _actorProvider.GetCurrent();

            var url = GetType().Name;

            _logger.Debug("Page: {url} initialized by {CurrentActor.Name}", url, CurrentActor.Name);
        }
    }
}

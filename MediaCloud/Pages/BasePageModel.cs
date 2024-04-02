using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.ConfigurationProvider;
using MediaCloud.WebApp.Services.DataService;
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
        protected IDataService _dataService;
        protected ILogger _logger;

        [BindProperty]
        public Actor? CurrentActor { get; set; }

        public AuthorizedPageModel(IDataService dataService) 
        {
            _dataService = dataService;
            _logger = LogManager.GetLogger("PageModel");

            LogPageInit();
        }

        private void LogPageInit()
        {
            CurrentActor = _dataService.GetCurrentActor();

            var url = GetType().Name;

            _logger.Debug("Page: {url} initialized by {CurrentActor.Name}", url, CurrentActor.Name);
        }
    }
}

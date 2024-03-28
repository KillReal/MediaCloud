using MediaCloud.WebApp.Services.DataService;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NLog;
using ILogger = NLog.ILogger;

namespace MediaCloud.WebApp.Pages
{
    public class BasePageModel : PageModel
    {
        protected IDataService _dataService;
        protected ILogger _logger;

        public BasePageModel(IDataService dataService) 
        {
            _dataService = dataService;
            _logger = LogManager.GetLogger("PageModel");

            LogPageInit();
        }

        private void LogPageInit()
        {
            var url = this.GetType().Name;
            var actorName = _dataService.GetCurrentActor().Name;

            _logger.Debug("Page: {url} initialized by {actorName}", url, actorName);
        }
    }
}

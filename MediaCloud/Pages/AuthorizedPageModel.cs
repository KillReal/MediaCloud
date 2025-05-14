using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.ConfigProvider;
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
        protected IUserProvider UserProvider;
        protected ILogger Logger;
        public User? CurrentUser { get; set; }
        public string UIThemeName { get; set; }

        public AuthorizedPageModel(IUserProvider userProvider, IConfigProvider configProvider) 
        {
            UserProvider = userProvider;
            Logger = LogManager.GetLogger("PageModel");

            UIThemeName = configProvider.UserSettings.UITheme;

            LogPageInit();
        }

        private void LogPageInit()
        {
            CurrentUser = UserProvider.GetCurrent();

            var url = GetType().Name;

            Logger.Debug("Page: {url} initialized by {CurrentActor.Name}", url, CurrentUser.Name);
        }
    }
}

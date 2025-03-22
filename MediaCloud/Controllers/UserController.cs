using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp.Services.UserProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace MediaCloud.WebApp.Controllers
{
    [Authorize]
    public class UserController(IUserProvider userProvider, IConfigProvider configProvider) : Controller
    {
        private readonly Logger _logger = LogManager.GetLogger("User");

        public IActionResult Logout()
        {
            userProvider.Logout(HttpContext);
            _logger.Info("Logout user with name: {AuthData.Name}", HttpContext.User.Identity?.Name);

            return Redirect("/");
        }

        public string GetUIThemeName()
        {
            return  configProvider.UserSettings.UITheme;
        }
    }
}
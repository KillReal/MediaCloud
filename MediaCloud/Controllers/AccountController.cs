using MediaCloud.WebApp.Services.UserProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using ILogger = NLog.ILogger;

namespace MediaCloud.WebApp.Controllers
{
    [Authorize]
    public class AccountController(IUserProvider actorProvider) : Controller
    {
        private readonly ILogger _logger = LogManager.GetLogger("Actor");
        private readonly IUserProvider _actorProvider = actorProvider;

        public IActionResult Logout()
        {
            _actorProvider.Logout(HttpContext);
            _logger.Info("Logout actor with name: {AuthData.Name}", HttpContext.User.Identity?.Name);

            return Redirect("/");
        }
    }
}
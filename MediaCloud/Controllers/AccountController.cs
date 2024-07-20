using MediaCloud.WebApp.Services.ActorProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using ILogger = NLog.ILogger;

namespace MediaCloud.WebApp.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly ILogger _logger;
        private readonly IActorProvider _actorProvider;

        public AccountController(IActorProvider actorProvider)
        {
            _logger = LogManager.GetLogger("Actor");
            _actorProvider = actorProvider;
        }

        public IActionResult Logout()
        {
            _actorProvider.Logout(HttpContext);
            _logger.Info("Logout actor with name: {AuthData.Name}", HttpContext.User.Identity?.Name);

            return Redirect("/");
        }
    }
}
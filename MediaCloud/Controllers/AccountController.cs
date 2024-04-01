using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

        public AccountController()
        {
            _logger = LogManager.GetLogger("Actor");
        }

        public IActionResult Logout()
        {
            _ = HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.Info("Logout actor with name: {AuthData.Name}", HttpContext.User.Identity?.Name);

            return Redirect("/");
        }
    }
}
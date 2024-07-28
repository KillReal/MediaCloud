using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp.Services.ConfigProvider;
using Microsoft.AspNetCore.Mvc;
using NLog;
using IConfigProvider = MediaCloud.WebApp.Services.ConfigProvider.IConfigProvider;

namespace MediaCloud.WebApp.Pages
{
    public class IndexModel : AuthorizedPageModel
    {
        private readonly IConfigProvider _configProvider;

        [BindProperty]
        public User User { get; set; }
        [BindProperty]
        public UserSettings UserSettings { get; set; }
        [BindProperty]
        public EnvironmentSettings? EnvironmentSettings { get; set; }
        [BindProperty]
        public bool IsEnvironmentSettingsChanged { get; set; }

        [BindProperty]
        public string ReturnUrl { get; set; } = "/";

        public IndexModel(IUserProvider userProvider, IConfigProvider configProvider) : base(userProvider)
        {
            _logger = LogManager.GetLogger("Actor");
            _configProvider = configProvider;

            User = userProvider.GetCurrent();

            UserSettings = _configProvider.UserSettings;
            EnvironmentSettings = User.IsAdmin 
                ? _configProvider.EnvironmentSettings 
                : null;
        }

        public IActionResult OnGet(string returnUrl = "/")
        {
            ReturnUrl = returnUrl;
            return Page();
        }

        public IActionResult OnPost()
        {
            _configProvider.UserSettings = UserSettings;

            var actualActor = _userProvider.GetCurrent();

            if (IsEnvironmentSettingsChanged && EnvironmentSettings != null && actualActor.IsAdmin)
            {
                _configProvider.EnvironmentSettings = EnvironmentSettings;
            }

            return Redirect(ReturnUrl);
        }
    }
}
    
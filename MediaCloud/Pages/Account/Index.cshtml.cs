using System.ComponentModel.DataAnnotations;
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
        public new User User { get; set; }
        [BindProperty]
        public UserSettings UserSettings { get; set; }
        [BindProperty]
        public EnvironmentSettings? EnvironmentSettings { get; set; }
        [BindProperty]
        public bool IsEnvironmentSettingsChanged { get; set; }

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

        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Redirect($"/Error?message={string.Join("\n", ModelState.Select(x => x.Value?.Errors).Where(y=>y?.Count>0).ToList())}");
            }
            
            _configProvider.UserSettings = UserSettings;

            var actualActor = _userProvider.GetCurrent();

            if (IsEnvironmentSettingsChanged && EnvironmentSettings != null && actualActor.IsAdmin)
            {
                _configProvider.EnvironmentSettings = EnvironmentSettings;
            }

            return Redirect("/Account");
        }
    }
}
    
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
        [BindProperty]
        public List<string> AutotaggingAiModels { get; set; }

        public IndexModel(IUserProvider userProvider, IConfigProvider configProvider, IAutotagService autotagService) 
            : base(userProvider, configProvider)
        {
            Logger = LogManager.GetLogger("User");
            _configProvider = configProvider;

            User = userProvider.GetCurrent();

            UserSettings = _configProvider.UserSettings;
            // EnvironmentSettings = User.IsAdmin 
            //     ? _configProvider.EnvironmentSettings 
            //     : null;

            AutotaggingAiModels = autotagService.GetAvailableModels();
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Redirect($"/Error?message={string.Join(",", errors)}");
            }
            
            _configProvider.UserSettings = UserSettings;

            var user = UserProvider.GetCurrent();

            if (IsEnvironmentSettingsChanged && EnvironmentSettings != null && user.IsAdmin)
            {
                _configProvider.EnvironmentSettings = EnvironmentSettings;
            }

            return Redirect("/User");
        }
    }
}
    
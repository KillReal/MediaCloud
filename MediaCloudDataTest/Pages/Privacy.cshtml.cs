using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediaCloud.Pages
{
    public class ChangelogModel : PageModel
    {
        private readonly ILogger<PrivacyModel> _logger;

        public ChangelogModel(ILogger<PrivacyModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}
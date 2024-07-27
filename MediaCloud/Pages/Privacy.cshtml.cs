using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.UserProvider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediaCloud.Pages
{
    public class ChangelogModel(IUserProvider actorProvider) : PageModel
    {
        [BindProperty]
        public User? CurrentActor { get; set; } = actorProvider.GetCurrentOrDefault();

        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
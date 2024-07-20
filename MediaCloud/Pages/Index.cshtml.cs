using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.ActorProvider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediaCloud.Pages
{
    public class IndexModel(IActorProvider actorProvider) : PageModel
    {
        [BindProperty]
        public Actor? CurrentActor { get; set; } = actorProvider.GetCurrentOrDefault();

        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
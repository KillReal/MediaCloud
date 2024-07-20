using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.ActorProvider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediaCloud.Pages
{
    public class ChangelogModel : PageModel
    {
        [BindProperty]
        public Actor? CurrentActor { get; set; }

        public ChangelogModel(IActorProvider actorProvider)
        { 
            CurrentActor = actorProvider.GetCurrentOrDefault();
        }

        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
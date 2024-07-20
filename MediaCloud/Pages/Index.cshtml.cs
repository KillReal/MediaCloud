using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.ActorProvider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediaCloud.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public Actor? CurrentActor { get; set; }

        public IndexModel(IActorProvider actorProvider)
        { 
            CurrentActor = actorProvider.GetCurrentOrDefault();
        }

        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
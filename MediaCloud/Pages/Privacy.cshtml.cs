using MediaCloud.Data;
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

        public ChangelogModel(IActorProvider actorProvider, AppDbContext context)
        { 
            CurrentActor = actorProvider.GetCurrent(context);
        }

        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
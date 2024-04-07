using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.ActorProvider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediaCloud.Pages
{
    public class PrivacyModel : PageModel
    {
        [BindProperty]
        public Actor? CurrentActor { get; set; }

        public PrivacyModel(IActorProvider actorProvider)
        { 
            CurrentActor = actorProvider.GetCurrentOrDefault();
        }

        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
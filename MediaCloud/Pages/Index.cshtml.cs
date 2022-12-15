using MediaCloud.WebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediaCloud.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> Logger;
        private readonly IActorProvider ActorProvider;

        public IndexModel(IActorProvider actorProvider, ILogger<IndexModel> logger)
        {
            Logger = logger;
            ActorProvider = actorProvider;
        }

        public IActionResult OnGet()
        {
            var currentActor = ActorProvider.GetCurrent() ?? new();

            if (currentActor.IsAdmin)
            {
                return Redirect("/Medias");
            }

            return Page();
        }
    }
}
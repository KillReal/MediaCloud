using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services;
using MediaCloud.WebApp.Services.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediaCloud.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> Logger;
        private readonly IActorProvider ActorProvider;
        private Actor CurrentActor;

        public IndexModel(IRepository repository, ILogger<IndexModel> logger)
        {
            Logger = logger;
            CurrentActor = repository.GetCurrentActor() ?? new();
        }

        public IActionResult OnGet()
        {
            if (CurrentActor.IsAdmin)
            {
                return Redirect("/Medias");
            }

            return Page();
        }
    }
}
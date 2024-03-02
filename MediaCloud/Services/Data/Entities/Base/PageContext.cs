using MediaCloud.Data;
using MediaCloud.WebApp.Services.ActorProvider;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediaCloud.Repositories
{
    public class PageContext<T> where T : PageModel
    {
        public AppDbContext Context { get; set; }
        public ILogger<T> Logger { get; set; }
        public IActorProvider ActorProvider { get; set; }

        public PageContext(AppDbContext context, ILogger<T> logger, IActorProvider actorProvider)
        {
            Context = context;
            Logger = logger;
            ActorProvider = actorProvider;
        }
    }
}

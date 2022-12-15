using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.Repository;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediaCloud.WebApp.Services.Repository.Entities.Base
{
    public class RepositoryContext
    {
        public AppDbContext Context { get; set; }
        public virtual ILogger Logger { get; set; }
        public Actor Actor { get; set; }

        public RepositoryContext(AppDbContext context, ILogger<Repository> logger, Actor actor)
        {
            Context = context;
            Logger = logger;
            Actor = actor;
        }
    }
}

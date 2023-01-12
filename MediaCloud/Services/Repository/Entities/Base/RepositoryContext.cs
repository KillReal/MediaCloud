using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.Repository;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediaCloud.WebApp.Services.Repository.Entities.Base
{
    public class RepositoryContext
    {
        public AppDbContext DbContext { get; set; }
        public virtual ILogger Logger { get; set; }
        public Actor? Actor { get; set; }

        public RepositoryContext(AppDbContext dbContext, ILogger<Repository> logger, Actor? actor)
        {
            DbContext = dbContext;
            Logger = logger;
            Actor = actor;
        }
    }
}

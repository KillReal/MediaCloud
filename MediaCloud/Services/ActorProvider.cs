using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using Microsoft.CodeAnalysis;

namespace MediaCloud.WebApp.Services
{
    public class ActorProvider : IActorProvider
    {
        private IHttpContextAccessor ContextAccessor;
        private AppDbContext DbContext;
        private Actor? CachedActor;

        public ActorProvider(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            DbContext = context;
            ContextAccessor = httpContextAccessor;
        }

        public Actor? GetCurrent()
        {
            var httpContext = ContextAccessor.HttpContext;

            if (httpContext == null)
            {
                return null;
            }

            var identity = httpContext.User.Identity;

            if (identity == null)
            {
                return null;
            }

            if (identity.Name == CachedActor?.Name)
            {
                return CachedActor;
            }

            CachedActor = new ActorRepository(DbContext).Get(identity.Name);

            return CachedActor;
        }
    }
}

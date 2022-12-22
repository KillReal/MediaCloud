using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;

namespace MediaCloud.WebApp.Services
{
    public class ActorProvider : IActorProvider
    {
        private IHttpContextAccessor _contextAccessor;
        private AppDbContext _context;
        private string CachedActorName;
        private Actor CachedActor;

        public ActorProvider(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _contextAccessor = httpContextAccessor;
        }

        public Actor? GetCurrent()
        {
            var httpContext = _contextAccessor.HttpContext;

            if (httpContext == null)
            {
                return null;
            }

            var identity = httpContext.User.Identity;

            if (identity == null)
            {
                return null;
            }

            if (identity.Name == CachedActorName)
            {
                return CachedActor;
            }

            CachedActorName = identity.Name;
            CachedActor = new ActorRepository(_context).Get(CachedActorName) ?? new();

            return CachedActor;
        }
    }
}

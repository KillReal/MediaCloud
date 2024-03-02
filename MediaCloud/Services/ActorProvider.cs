using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.DataService.Entities.Base;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediaCloud.WebApp.Services
{
    public class ActorProvider : IActorProvider
    {
        private IHttpContextAccessor ContextAccessor;
        private IServiceScopeFactory ScopeFactory;
        private Actor? CachedActor;

        public ActorProvider(IServiceScopeFactory scopeFactory, IHttpContextAccessor httpContextAccessor)
        {
            ScopeFactory = scopeFactory;
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

            using var scope = ScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            CachedActor = new ActorDataService(dbContext).Get(identity.Name);

            return CachedActor;
        }
    }
}

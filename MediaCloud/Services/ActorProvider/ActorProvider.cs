using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.DataService.Entities.Base;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediaCloud.WebApp.Services.ActorProvider
{
    public class ActorProvider : IActorProvider
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IServiceScope _scope;
        private Actor? _cachedActor;

        public ActorProvider(IServiceScopeFactory scopeFactory, IHttpContextAccessor httpContextAccessor)
        {
            _scope = scopeFactory.CreateScope();
            _contextAccessor = httpContextAccessor;
        }

        public Actor? GetCurrent()
        {
            var newContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
            return GetCurrent(newContext);
        }

        public Actor? GetCurrent(AppDbContext context)
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

            if (identity.Name == _cachedActor?.Name)
            {
                return _cachedActor;
            }

            _cachedActor = new ActorRepository(context).Get(identity.Name);

            return _cachedActor;
        }
    }
}

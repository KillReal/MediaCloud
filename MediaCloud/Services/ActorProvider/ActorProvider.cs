using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.DataService.Entities.Base;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace MediaCloud.WebApp.Services.ActorProvider
{
    public class ActorProvider : IActorProvider
    {
        private readonly IStatisticService _statisticService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly AppDbContext _context;
        private Actor? _cachedActor;

        public ActorProvider(IServiceScopeFactory scopeFactory, IHttpContextAccessor httpContextAccessor, IStatisticService statisticService)
        {
            _context = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
            _contextAccessor = httpContextAccessor;
            _statisticService = statisticService;
        }

        public Actor? GetCurrent()
        {
            return GetCurrent(_context);
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

        public bool AuthorizeByAuthData(AuthData data, HttpContext httpContext)
        {
            var actor = _context.Actors.FirstOrDefault(x => x.Name == data.Name && x.IsActivated);

            if (actor == null || actor.PasswordHash == null || SecureHash.Verify(data.Password, actor.PasswordHash) == false)
            {
                return false;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, data.Name)

            };
            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");

            httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            actor.UpdatedAt = actor.UpdatedAt.ToUniversalTime();
            actor.CreatedAt = actor.CreatedAt.ToUniversalTime();
            actor.LastLoginAt = actor.LastLoginAt.ToUniversalTime();

            UpdateLastLoginAt();

            _statisticService.ActivityFactorRaised.Invoke();

            _cachedActor = actor;

            return true;
        }

        private bool UpdateLastLoginAt()
        {
            var actor = GetCurrent(_context);

            if (actor == null)
            {
                return false;   
            }

            actor.LastLoginAt = DateTime.Now.ToUniversalTime();

            new ActorRepository(_context).Update(actor);
            return true;
        }
    }
}

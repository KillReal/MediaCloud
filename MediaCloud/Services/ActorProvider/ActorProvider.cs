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
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly AppDbContext _context;
        private Actor? _cachedActor;

        public ActorProvider(IServiceScopeFactory scopeFactory, IHttpContextAccessor httpContextAccessor)
        {
            _context = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
            _contextAccessor = httpContextAccessor;
        }

        public Actor? GetCurrent(AppDbContext context)
        {
            var httpContext = _contextAccessor.HttpContext;

            if (httpContext == null)
            {
                return null;
            }

            var identity = httpContext.User.Identity;

            if (identity == null || identity.Name == null)
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

        public bool Authorize(AuthData data, HttpContext httpContext)
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
            actor.LastLoginAt = DateTime.Now.ToUniversalTime();

            _cachedActor = actor;

            return true;
        }

        public RegistrationResult Register(AuthData data, string inviteCode)
        {
            var actor = _context.Actors.FirstOrDefault(x => x.InviteCode == inviteCode && x.IsActivated == false);

            if (actor == null)
            {
                return new(false, $"Join attempt failed due to incorrect invite code: {inviteCode}");
            }

            if (_context.Actors.Any(x => x.Name == data.Name))
            {
                return new(false, $"Join attempt failed due to already existing name: {data.Name}");
            }

            if (data.Password.Length < 6)
            {
                return new(false, $"Join attempt failed due to short password, it must be longer than 6 characters");
            }

            actor.Name = data.Name;
            actor.PasswordHash = SecureHash.Hash(data.Password);
            actor.IsActivated = true;

            _context.Actors.Update(actor);
            _context.SaveChanges();

            return new(true, $"Joined in by {data.Name} and invite code: {inviteCode}");
        }

        public bool SaveSettings(string jsonSettings)
        {
            var currentActor = GetCurrent(_context);

            if (currentActor != null) {
                currentActor.PersonalSettings = jsonSettings;
                _context.Actors.Update(currentActor);
                _context.SaveChanges();

                return true;
            }

            return false;
        }
    }
}

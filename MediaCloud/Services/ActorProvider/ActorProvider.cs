﻿using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.TaskScheduler.Tasks;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NLog;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using Task = MediaCloud.TaskScheduler.Tasks.Task;
using Newtonsoft.Json;

namespace MediaCloud.WebApp.Services.ActorProvider
{
    public class ActorProvider : IActorProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private IMemoryCache _memoryCache;
        private Mutex _mutex = new();

        private MemoryCacheEntryOptions _memoryCacheOptions;

        public ActorProvider(IServiceScopeFactory scopeFactory, IHttpContextAccessor httpContextAccessor, 
            IMemoryCache memoryCache, IConfiguration configuration)
        {
            var scope = scopeFactory.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            _memoryCache = memoryCache;
            _memoryCacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(new EnvironmentSettings(configuration).CookieExpireTime));

            _httpContextAccessor = httpContextAccessor;

            LogManager.GetLogger("ActorProvider").Debug("ActorProvider initialized");
        }

        public Actor GetCurrent()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                throw new ArgumentException("Cannot get actor without HttpContext");
            }

            var identity = httpContext.User.Identity;

            if (identity == null || identity.Name == null)
            {
                throw new ArgumentException("Cannot get actor without HttpContext");
            }

            if (_memoryCache.TryGetValue(identity.Name, out Actor actor))
            {
                return actor;
            }

            _mutex.WaitOne();

            var cachedActor = _context.Actors.First(x => x.Name == identity.Name);
            _memoryCache.Set(identity.Name, cachedActor, _memoryCacheOptions);

            _mutex.ReleaseMutex();

            return cachedActor;
        }

        public Actor? GetCurrentOrDefault()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                return null;
            }

            var identity = httpContext.User.Identity;

            if (identity == null || identity.Name == null)
            {
                return null;
            }

            if (_memoryCache.TryGetValue(identity.Name, out Actor actor))
            {
                return actor;
            }

            var cachedActor = _context.Actors.First(x => x.Name == identity.Name);
            _memoryCache.Set(identity.Name, cachedActor, _memoryCacheOptions);

            return cachedActor;
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
                new(ClaimTypes.Name, data.Name)
            };
            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");

            httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            actor.UpdatedAt = actor.UpdatedAt.ToUniversalTime();
            actor.CreatedAt = actor.CreatedAt.ToUniversalTime();
            actor.LastLoginAt = DateTime.Now.ToUniversalTime();
            
            _context.Actors.Update(actor);
            _memoryCache.Set(data.Name, actor, _memoryCacheOptions);

            return true;
        }

        public void Logout(HttpContext httpContext)
        {
            _ = httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var identity = httpContext.User.Identity;

            if (identity == null || identity.Name == null)
            {
                throw new ArgumentException("Cannot get actor without HttpContext");
            }

            if (_memoryCache.TryGetValue(identity.Name, out Actor actor))
            {
                _memoryCache.Remove(identity.Name);
            }
        }

        public RegistrationResult Register(IConfigProvider configProvider, AuthData data, string inviteCode)
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

            if (data.Password.Length < configProvider.EnvironmentSettings.PasswordMinLength)
            {
                return new(false, $"Join attempt failed due to short password, it must be longer than 6 characters");
            }

            if (data.Password.Any(x => char.IsSymbol(x)) == false && configProvider.EnvironmentSettings.PasswordMustHaveSymbols)
            {
                return new(false, $"Join attempt failed due to week password, it must contain at least one symbol");
            }

            actor.Name = data.Name;
            actor.PasswordHash = SecureHash.Hash(data.Password);
            actor.IsActivated = true;

            _context.Actors.Update(actor);
            _context.SaveChanges();

            return new(true, $"Joined in by {data.Name} and invite code: {inviteCode}");
        }

        public ActorSettings? GetSettings()
        {
            var currentActor = GetCurrent();

            if (currentActor != null && currentActor.PersonalSettings != null)
            {
                return JsonConvert.DeserializeObject<ActorSettings>(currentActor.PersonalSettings);
            }

            return null;
        }

        public bool SaveSettings(string jsonSettings)
        {
            var currentActor = GetCurrent();

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

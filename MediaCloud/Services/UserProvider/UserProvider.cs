using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.ConfigProvider;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;
using NLog;
using System.Security.Claims;
using Newtonsoft.Json;

namespace MediaCloud.WebApp.Services.UserProvider
{
    public class UserProvider : IUserProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly Mutex _mutex = new();

        private readonly MemoryCacheEntryOptions _memoryCacheOptions;

        public UserProvider(IServiceScopeFactory scopeFactory, IHttpContextAccessor httpContextAccessor, 
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

        public User GetCurrent()
        {
            var httpContext = _httpContextAccessor.HttpContext ?? 
                throw new ArgumentException("Cannot get actor without HttpContext");
            
            var identity = httpContext.User.Identity;

            if (identity == null || identity.Name == null)
            {
                throw new ArgumentException("Cannot get actor without HttpContext");
            }

            if (_memoryCache.TryGetValue(identity.Name, out User? actor) && actor != null)
            {
                return actor;
            }

            _mutex.WaitOne();

            var cachedUser = _context.Users.First(x => x.Name == identity.Name);
            _memoryCache.Set(identity.Name, cachedUser, _memoryCacheOptions);

            _mutex.ReleaseMutex();

            return cachedUser;
        }

        public User? GetCurrentOrDefault()
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

            if (_memoryCache.TryGetValue(identity.Name, out User? user))
            {
                return user;
            }

            var cachedUser = _context.Users.First(x => x.Name == identity.Name);
            _memoryCache.Set(identity.Name, cachedUser, _memoryCacheOptions);

            return cachedUser;
        }

        public bool Authorize(AuthData data, HttpContext httpContext)
        {
            var user = _context.Users.FirstOrDefault(x => x.Name == data.Name && x.IsActivated);

            if (user == null || user.PasswordHash == null || SecureHash.Verify(data.Password, user.PasswordHash) == false)
            {
                return false;
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, data.Name)
            };
            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");

            httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            user.UpdatedAt = user.UpdatedAt.ToUniversalTime();
            user.CreatedAt = user.CreatedAt.ToUniversalTime();
            user.LastLoginAt = DateTime.Now.ToUniversalTime();
            
            _context.Users.Update(user);
            _memoryCache.Set(data.Name, user, _memoryCacheOptions);

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

            if (_memoryCache.TryGetValue(identity.Name, out User? _))
            {
                _memoryCache.Remove(identity.Name);
            }
        }

        public RegistrationResult Register(IConfigProvider configProvider, AuthData data, string inviteCode)
        {
            var user = _context.Users.FirstOrDefault(x => x.InviteCode == inviteCode && x.IsActivated == false);

            if (user == null)
            {
                return new(false, $"Join attempt failed due to incorrect invite code: {inviteCode}");
            }

            if (_context.Users.Any(x => x.Name == data.Name))
            {
                return new(false, $"Join attempt failed due to already existing name: {data.Name}");
            }

            if (data.Password.Length < configProvider.EnvironmentSettings.PasswordMinLength)
            {
                return new(false, $"Join attempt failed due to short password, it must be longer than 6 characters");
            }

            if (data.Password.Any(char.IsSymbol) == false && configProvider.EnvironmentSettings.PasswordMustHaveSymbols)
            {
                return new(false, $"Join attempt failed due to week password, it must contain at least one symbol");
            }

            user.Name = data.Name;
            user.PasswordHash = SecureHash.Hash(data.Password);
            user.IsActivated = true;

            _context.Users.Update(user);
            _context.SaveChanges();

            return new(true, $"Joined in by {data.Name} and invite code: {inviteCode}");
        }

        public UserSettings? GetSettings()
        {
            var currentUser = GetCurrent();

            if (currentUser != null && currentUser.PersonalSettings != null)
            {
                return JsonConvert.DeserializeObject<UserSettings>(currentUser.PersonalSettings);
            }

            return null;
        }

        public bool SaveSettings(string jsonSettings)
        {
            var currentUser = GetCurrent();

            if (currentUser != null) {
                currentUser.PersonalSettings = jsonSettings;
                _context.Users.Update(currentUser);
                _context.SaveChanges();

                return true;
            }

            return false;
        }
    }
}

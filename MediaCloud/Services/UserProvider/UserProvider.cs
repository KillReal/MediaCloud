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
        private readonly EnvironmentSettings _environmentSettings; 

        private readonly MemoryCacheEntryOptions _memoryCacheOptions;

        public UserProvider(IServiceScopeFactory scopeFactory, IHttpContextAccessor httpContextAccessor, 
            IMemoryCache memoryCache, IConfiguration configuration)
        {
            var scope = scopeFactory.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            _memoryCache = memoryCache;
            _environmentSettings = new EnvironmentSettings(configuration);
            _memoryCacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(_environmentSettings.CookieExpireTime));

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

            _mutex.WaitOne();

            var cachedUser = _context.Users.First(x => x.Name == identity.Name);
            _memoryCache.Set(identity.Name, cachedUser, _memoryCacheOptions);

            _mutex.ReleaseMutex();

            return cachedUser;
        }

        public AuthorizationResult Authorize(AuthData data, HttpContext httpContext)
        {
            var user = _context.Users.FirstOrDefault(x => x.Name == data.Name && x.IsActivated);

            if (user == null || user.PasswordHash == null)
            {
                return new(false, "Invalid data.");
            }

            _context.Entry(user).Reload();

            if (_environmentSettings.LimitLoginAttempts && user.NextLoginAttemptAt > DateTime.Now)
            {
                var time = (int)(user.NextLoginAttemptAt - DateTime.Now).Value.TotalMinutes + 1;
                return new(false, $"Account locked due to run out of attempts.\r\nNext attempt in {time} minute(-s).");
            }

            if (SecureHash.Verify(data.Password, user.PasswordHash) == false)
            {
                if (_environmentSettings.LimitLoginAttempts) 
                {
                    user.FailLoginAttemptCount += 1;
                    var freeAttemptCount = 3;

                    if (user.FailLoginAttemptCount >= freeAttemptCount)
                    {
                        var duration = Enumerable.Range(freeAttemptCount, user.FailLoginAttemptCount - freeAttemptCount + 1).Sum();
                        user.NextLoginAttemptAt = DateTime.Now.AddMinutes(duration);
                    }
                }

                _context.Users.Update(user);
                _context.SaveChanges();
                
                return new(false, $"Incorrect username or password.");
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
            user.FailLoginAttemptCount = 0;
            user.NextLoginAttemptAt = null;
            
            _context.Users.Update(user);
            _context.SaveChanges();
            _memoryCache.Set(data.Name, user, _memoryCacheOptions);

            return new (true, $"Successfully authorized for {user.Name}");
        }

        public void Logout(HttpContext httpContext)
        {
            var identity = httpContext.User.Identity;

            if (identity == null || identity.Name == null)
            {
                throw new ArgumentException("Cannot get actor without HttpContext");
            }

            if (_memoryCache.TryGetValue(identity.Name, out User? _))
            {
                _memoryCache.Remove(identity.Name);
            }

            _ = httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public RegistrationResult Register(AuthData data, string inviteCode)
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

            if (data.Password.Length < _environmentSettings.PasswordMinLength)
            {
                return new(false, $"Join attempt failed due to short password, it must be longer than 6 characters");
            }

            if (data.Password.Any(char.IsSymbol) == false && _environmentSettings.PasswordMustHaveSymbols)
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
            var currentUser = GetCurrentOrDefault();

            if (_memoryCache.TryGetValue($"settings-{currentUser?.Id}", out UserSettings? settings))
            {
                return settings;
            }

            if (currentUser is not { PersonalSettings: not null })
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(currentUser.PersonalSettings))
            {
                return null;
            }
            
            settings = JsonConvert.DeserializeObject<UserSettings>(currentUser.PersonalSettings);
            _memoryCache.Set($"settings-{currentUser.Id}", settings, _memoryCacheOptions);

            return settings;

        }

        public bool SaveSettings(string jsonSettings)
        {
            var currentUser = GetCurrentOrDefault();

            if (currentUser == null)
            {
                return false;
            }
            
            currentUser.PersonalSettings = jsonSettings;
            _context.Users.Update(currentUser);
            _context.SaveChanges();

            if (!_memoryCache.TryGetValue($"settings-{currentUser?.Id}", out UserSettings? settings))
            {
                return true;
            }
            
            if (settings is not null)
            {
                _memoryCache.Remove($"settings-{currentUser?.Id}");
            }

            return true;

        }
    }
}

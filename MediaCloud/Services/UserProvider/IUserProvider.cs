using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.ConfigProvider;

namespace MediaCloud.WebApp.Services.UserProvider
{
    public interface IUserProvider
    {
        public User GetCurrent();
        public User? GetCurrentOrDefault();
        public AuthorizationResult Authorize(AuthData data, HttpContext httpContext);
        public void Logout(HttpContext httpContext);
        public RegistrationResult Register(AuthData data, string inviteCode);
         public UserSettings? GetSettings();
        public bool SaveSettings(string jsonSettings);
        public void CleanCache();
        public bool TryCleanCacheForUser(Guid userId);
    }
}

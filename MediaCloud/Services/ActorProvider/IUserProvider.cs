using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.ConfigProvider;

namespace MediaCloud.WebApp.Services.UserProvider
{
    public interface IUserProvider
    {
        public User GetCurrent();
        public User? GetCurrentOrDefault();
        public bool Authorize(AuthData data, HttpContext httpContext);
        public void Logout(HttpContext httpContext);
        public RegistrationResult Register(IConfigProvider configProvider, AuthData data, string inviteCode);
         public ActorSettings? GetSettings();
        public bool SaveSettings(string jsonSettings);
    }
}

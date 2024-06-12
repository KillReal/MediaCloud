using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.ConfigurationProvider;

namespace MediaCloud.WebApp.Services.ActorProvider
{
    public interface IActorProvider
    {
        public Actor GetCurrent();
        public Actor? GetCurrentOrDefault();
        public bool Authorize(AuthData data, HttpContext httpContext);
        public void Logout(HttpContext httpContext);
        public RegistrationResult Register(IConfigProvider configProvider, AuthData data, string inviteCode);
        public bool SaveSettings(string jsonSettings);
    }
}

using MediaCloud.Data;
using MediaCloud.Data.Models;

namespace MediaCloud.WebApp.Services.ActorProvider
{
    public interface IActorProvider
    {
        public Actor? GetCurrent(AppDbContext context);
        public bool Authorize(AuthData data, HttpContext httpContext);
        public RegistrationResult Register(AuthData data, string inviteCode);
    }
}

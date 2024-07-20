using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.ConfigProvider;
using NLog;

namespace MediaCloud.WebApp.Services.ActorProvider
{
    public class DummyActorProvider : IActorProvider
    {
        private readonly ActorRepository _actorRepository;
        private readonly Actor _currentActor;

        public DummyActorProvider(Actor actor, ActorRepository actorRepository)
        {
            _actorRepository = actorRepository;
            _currentActor = actor;

            LogManager.GetLogger("ActorProvider").Debug("DummyActorProvider initialized");
        }

        public Actor GetCurrent() => _currentActor;
        
        public Actor? GetCurrentOrDefault() => _currentActor;

        public bool Authorize(AuthData data, HttpContext httpContext) => false;

        public void Logout(HttpContext httpContext)
        {
            throw new NotImplementedException($"Dummy provider cannot proceed logout");
        }

        public RegistrationResult Register(IConfigProvider configProvider, AuthData data, string inviteCode) 
        => new(false, $"Dummy provider cannot register an actor");

        public bool SaveSettings(string jsonSettings)
        {
             _currentActor.PersonalSettings = jsonSettings;
            _actorRepository.Update(_currentActor);

            return true;
        }

        public ActorSettings? GetSettings()
        {
            throw new NotImplementedException();
        }
    }
}

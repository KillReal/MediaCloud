using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.ConfigProvider;
using NLog;

namespace MediaCloud.WebApp.Services.UserProvider
{
    public class DummyUserProvider : IUserProvider
    {
        private readonly UserRepository _userRepository;
        private readonly User _currentUser;

        public DummyUserProvider(User user, UserRepository userRepository)
        {
            _userRepository = userRepository;
            _currentUser = user;

            LogManager.GetLogger("ActorProvider").Debug("DummyActorProvider initialized");
        }

        public User GetCurrent() => _currentUser;
        
        public User? GetCurrentOrDefault() => _currentUser;

        public AuthorizationResult Authorize(AuthData data, HttpContext httpContext) 
            => new(false, "Dummy provider cannot register an actor");

        public void Logout(HttpContext httpContext)
        {
            throw new NotImplementedException($"Dummy provider cannot proceed logout");
        }

        public RegistrationResult Register(AuthData data, string inviteCode) 
            => new(false, "Dummy provider cannot register an actor");

        public bool SaveSettings(string jsonSettings)
        {
            _currentUser.PersonalSettings = jsonSettings;
            _userRepository.Update(_currentUser);

            return true;
        }

        public UserSettings? GetSettings()
        {
            throw new NotImplementedException();
        }
    }
}

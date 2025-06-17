using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.ConfigProvider;
using Newtonsoft.Json;
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
        {
            return new AuthorizationResult(false, "Dummy provider cannot register an actor");
        }

        public void Logout(HttpContext httpContext)
        {
            throw new NotImplementedException($"Dummy provider cannot proceed logout");
        }

        public RegistrationResult Register(AuthData data, string inviteCode)
        {
            return new RegistrationResult(false, "Dummy provider cannot register an actor");
        }

        public bool SaveSettings(string jsonSettings)
        {
            _currentUser.PersonalSettings = jsonSettings;
            _userRepository.Update(_currentUser);

            return true;
        }

        public UserSettings? GetSettings()
        {
            if (_currentUser.PersonalSettings == null)
            {
                return null;    
            }
            
            return JsonConvert.DeserializeObject<UserSettings>(_currentUser.PersonalSettings);
        }
        
        public void CleanCache()
        {
            throw new NotImplementedException($"Dummy provider cannot clean cache");
        }

        public bool TryCleanCacheForUser(Guid userId)
        {
            throw new NotImplementedException($"Dummy provider cannot clean cache");
        }

    }
}

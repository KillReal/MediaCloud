using MediaCloud.WebApp.Services.UserProvider;
using Newtonsoft.Json;
using NLog;
using ILogger = NLog.ILogger;

namespace MediaCloud.WebApp.Services.ConfigProvider
{
    public class ConfigProvider : IConfigProvider
    {
        private readonly IUserProvider _userProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        private EnvironmentSettings _environmentSettings;

        /// <summary>
        /// Direct access to a settings of current actor. Default values are taken from <see cref="IConfiguration"/>.
        /// Does not save changes to database implicitly. 
        /// Use <see cref="SaveUserSettings"/> for explicit save.
        /// </summary>
        public UserSettings UserSettings
        { 
            get => _userProvider.GetSettings() ?? new UserSettings(_configuration);
            set => SaveUserSettings(value);
        }
        
        /// <summary>
        /// Direct access to a settings of environment. Default values are taken from <see cref="IConfiguration"/>.
        /// Does not save changes to database implicitly. 
        /// Use <see cref="SaveEnvironmentSettings()"/> for explicit save.
        /// </summary>
        public EnvironmentSettings EnvironmentSettings 
        { 
            get => _environmentSettings;
            set
            {
                _environmentSettings = value;
                SaveEnvironmentSettings();
            } 
        }


        public ConfigProvider(IConfiguration configuration, IUserProvider userProvider)
        {
            _userProvider = userProvider;
            _configuration = configuration;
            _logger = LogManager.GetLogger("ConfigurationProvider");

            var actor = _userProvider.GetCurrentOrDefault();

            if (actor != null)
            {
                _logger.Debug("Initialized ConfigurationProvider by actor: {0}", actor.Name);
            }
            else 
            {
                _logger.Debug("Initialized ConfigurationProvider anonymously");
            }

            _environmentSettings = new EnvironmentSettings(_configuration);
        }

        /// <summary>
        /// Implicitly saves changes to database in json format.
        /// </summary>
        /// <returns> Result of operation. </returns>
        public bool SaveUserSettings(UserSettings settings)
        {
            var jsonSettings = JsonConvert.SerializeObject(settings, Formatting.Indented);
            return _userProvider.SaveSettings(jsonSettings);
        }

        /// <summary>
        /// Implicitly saves changes to database in json format.
        /// </summary>
        /// <returns> Result of operation. </returns>
        public bool SaveEnvironmentSettings()
        {
            try 
            {
                EnvironmentSettings.SaveToAppSettings(_configuration);
                return true;
            }
            catch (Exception ex) 
            {
                _logger.Error(ex, "Could not save environment settings.");
                return false;  
            }
        }
    }
}
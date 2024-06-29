using MediaCloud.WebApp.Services.ActorProvider;
using Newtonsoft.Json;
using NLog;
using ILogger = NLog.ILogger;

namespace MediaCloud.WebApp.Services.ConfigProvider
{
    public class ConfigProvider : IConfigProvider
    {
        private readonly IActorProvider _actorProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        private EnvironmentSettings _environmentSettings;

        /// <summary>
        /// Direct access to a settings of current actor. Default values are taken from <see cref="IConfiguration"/>.
        /// Does not save changes to database implicitly. 
        /// Use <see cref="SaveActorSettings()"/> for explicit save.
        /// </summary>
        public ActorSettings ActorSettings
        { 
            get 
            { 
                return _actorProvider.GetSettings() ?? new(_configuration);
            }
            set 
            {
                SaveActorSettings();
            }
        }
        
        /// <summary>
        /// Direct access to a settings of environment. Default values are taken from <see cref="IConfiguration"/>.
        /// Does not save changes to database implicitly. 
        /// Use <see cref="SaveEnvironmentSettings()"/> for explicit save.
        /// </summary>
        public EnvironmentSettings EnvironmentSettings 
        { 
            get
            {
                return _environmentSettings;
            }
            set
            {
                _environmentSettings = value;
                SaveEnvironmentSettings();
            } 
        }


        public ConfigProvider(IConfiguration configuration, IActorProvider actorProvider)
        {
            _actorProvider = actorProvider;
            _configuration = configuration;
            _logger = LogManager.GetLogger("ConfigurationProvider");

            var actor = _actorProvider.GetCurrentOrDefault();

            if (actor != null)
            {
                _logger.Debug("Initialized ConfigurationProvider by actor: {0}", actor.Name);
            }
            else 
            {
                _logger.Debug("Initialized ConfigurationProvider anonymously");
            }

            _environmentSettings = new(_configuration);
        }

        /// <summary>
        /// Implicitly saves changes to database in json format.
        /// </summary>
        /// <returns> Result of operation. </returns>
        public bool SaveActorSettings()
        {
            var jsonSettings = JsonConvert.SerializeObject(ActorSettings, Formatting.Indented);
            return _actorProvider.SaveSettings(jsonSettings);
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
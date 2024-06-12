using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.ActorProvider;
using Newtonsoft.Json;
using NLog;
using ILogger = NLog.ILogger;

namespace MediaCloud.WebApp.Services.ConfigurationProvider 
{
    public class ConfigProvider : IConfigProvider
    {
        private IActorProvider _actorProvider;
        private IConfiguration _configuration;
        private ILogger _logger;

        private ActorSettings ParseActorSettings(Actor actor)
        {
            try 
            {
                return  JsonConvert.DeserializeObject<ActorSettings>(actor.PersonalSettings ?? "") 
                    ?? new(_configuration);
            }
            catch (Exception ex) {
                _logger.Error("Could not parse personal settings for actor {0} exception: {1}", actor.Name, ex);
                return new(_configuration);
            }
        }

        /// <summary>
        /// Direct access to a settings of current actor. Default values are taken from <see cref="IConfiguration"/>.
        /// Does not save changes to database implicitly. 
        /// Use <see cref="SaveActorSettings()"/> for explicit save.
        /// </summary>
        public ActorSettings ActorSettings { get; set; }
        /// <summary>
        /// Direct access to a settings of environment. Default values are taken from <see cref="IConfiguration"/>.
        /// Does not save changes to database implicitly. 
        /// Use <see cref="SaveEnvironmentSettings()"/> for explicit save.
        /// </summary>
        public EnvironmentSettings EnvironmentSettings { get; set; }

        public ConfigProvider(IConfiguration configuration, IActorProvider actorProvider)
        {
            _actorProvider = actorProvider;
            _configuration = configuration;
            _logger = LogManager.GetLogger("ConfigurationProvider");

            var actor = _actorProvider.GetCurrentOrDefault();

            if (actor != null)
            {
                _logger.Debug("Initialized ConfigurationProvider by actor: {0}", actor.Name);
                ActorSettings = ParseActorSettings(actor);
            }
            else 
            {
                _logger.Debug("Initialized ConfigurationProvider anonymously");
                ActorSettings = new(_configuration);
            }

            EnvironmentSettings = new(_configuration);
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
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

        public ActorSettings ActorSettings { get; set; }
        public EnvironmentSettings EnvironmentSettings { get; set; }

        public ConfigProvider(IConfiguration configuration, IActorProvider actorProvider, AppDbContext context)
        {
            _actorProvider = actorProvider;
            _configuration = configuration;
            _logger = LogManager.GetLogger("ConfigurationProvider");

            var currentActor = _actorProvider.GetCurrent(context);

            if (currentActor != null)
            {
                _logger.Debug("Initialized ConfigurationProvider by actor: {0}", currentActor.Name);
                ActorSettings = ParseActorSettings(currentActor);
            }
            else 
            {
                _logger.Debug("Initialized ConfigurationProvider anonymously");
                ActorSettings = new(_configuration);
            }

            EnvironmentSettings = new(_configuration);
        }

        public bool SaveActorSettings()
        {
            var jsonSettings = JsonConvert.SerializeObject(ActorSettings, Formatting.Indented);
            return _actorProvider.SaveSettings(jsonSettings);
        }
    }
}
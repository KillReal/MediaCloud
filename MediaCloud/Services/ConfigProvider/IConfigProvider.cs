using MediaCloud.WebApp.Services.ActorProvider;

namespace MediaCloud.WebApp.Services.ConfigurationProvider
{
    public interface IConfigProvider
    {
        public ActorSettings ActorSettings { get; set;}
        public EnvironmentSettings EnvironmentSettings { get; set; }
        public bool SaveActorSettings();
        public bool SaveEnvironmentSettings();
    }
}
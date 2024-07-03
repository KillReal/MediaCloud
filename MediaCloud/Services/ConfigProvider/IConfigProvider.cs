using MediaCloud.WebApp.Services.ActorProvider;

namespace MediaCloud.WebApp.Services.ConfigProvider
{
    public interface IConfigProvider
    {
        public ActorSettings ActorSettings { get; set;}
        public EnvironmentSettings EnvironmentSettings { get; set; }
        public bool SaveActorSettings(ActorSettings settings);
        public bool SaveEnvironmentSettings();
    }
}
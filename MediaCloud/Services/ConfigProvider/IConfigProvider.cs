namespace MediaCloud.WebApp.Services.ConfigProvider
{
    public interface IConfigProvider
    {
        public UserSettings UserSettings { get; set;}
        public EnvironmentSettings EnvironmentSettings { get; set; }
        public bool SaveActorSettings(UserSettings settings);
        public bool SaveEnvironmentSettings();
    }
}
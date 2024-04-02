namespace MediaCloud.WebApp.Services.ConfigurationProvider 
{
    [Serializable]
    public class EnvironmentSettings
    {
        public string? DatabaseConnectionString {get; set;}
        public int CookieExpireTime {get; set;}
        public int PreviewMaxHeight {get; set;}
        public int TaskSchedulerWorkerCount {get; set;}
        public int PasswordMinLength {get; set;}
        public bool PasswordMustHaveSymbols {get; set;}

        public EnvironmentSettings(IConfiguration configuration)
        {
            DatabaseConnectionString = configuration?["ConnectionStrings:Database"];
            CookieExpireTime = configuration.GetValue<int>("CookieExpireTime");
            PreviewMaxHeight = configuration.GetValue<int>("PreviewMaxHeight");
            TaskSchedulerWorkerCount = configuration.GetValue<int>("TaskSchedulerWorkerCount");
            PasswordMinLength = configuration.GetValue<int>("PasswordMinLength");
            PasswordMustHaveSymbols = configuration.GetValue<bool>("PasswordMustHaveSymbols");
        }

        public EnvironmentSettings()
        {
            
        }
    }
}
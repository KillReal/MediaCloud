namespace MediaCloud.WebApp.Services.ConfigurationProvider 
{
    [Serializable]
    public class EnvironmentSettings
    {
        public string? DatabaseConnectionString {get; set;}
        public int CookieExpireTime {get; set;}
        public int PreviewMaxHeight {get; set;}
        public int TaskSchedulerWorkerCount {get; set;}

        public EnvironmentSettings(IConfiguration configuration)
        {
            var dbPath = configuration?["ConnectionStrings:Database"];
            DatabaseConnectionString =  dbPath?.Split(";").First().Split("=").Last() ?? "";
            CookieExpireTime = configuration.GetValue<int>("CookieExpireTime");
            PreviewMaxHeight = configuration.GetValue<int>("PreviewMaxHeight");
            TaskSchedulerWorkerCount = configuration.GetValue<int>("TaskSchedulerWorkerCount");
        }

        public EnvironmentSettings()
        {
            
        }
    }
}
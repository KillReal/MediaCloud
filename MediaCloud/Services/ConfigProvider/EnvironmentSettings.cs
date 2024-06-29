using SixLabors.ImageSharp;

namespace MediaCloud.WebApp.Services.ConfigProvider 
{
    [Serializable]
    public class EnvironmentSettings
    {
        public string? DatabaseConnectionString {get; set;}
        public string? AiJoyTagConnectionString {get; set;}
        public int CookieExpireTime {get; set;}
        public int PreviewMaxHeight {get; set;}
        public int TaskSchedulerWorkerCount {get; set;}
        public int TaskSchedulerAutotaggingWorkerCount {get; set;}
        public int PasswordMinLength {get; set;}
        public bool PasswordMustHaveSymbols {get; set;}

        public EnvironmentSettings(IConfiguration configuration)
        {
            DatabaseConnectionString = configuration?["ConnectionStrings:Database"];
            AiJoyTagConnectionString = configuration?["ConnectionStrings:AiJoyTag"];
            CookieExpireTime = configuration.GetValue<int>("CookieExpireTime");
            PreviewMaxHeight = configuration.GetValue<int>("PreviewMaxHeight");
            TaskSchedulerWorkerCount = configuration.GetValue<int>("TaskSchedulerWorkerCount");
            TaskSchedulerAutotaggingWorkerCount = configuration.GetValue<int>("TaskSchedulerAutotaggingWorkerCount");
            PasswordMinLength = configuration.GetValue<int>("PasswordMinLength");
            PasswordMustHaveSymbols = configuration.GetValue<bool>("PasswordMustHaveSymbols");
        }

        public EnvironmentSettings()
        {
            
        }

        public void SaveToAppSettings(IConfiguration configuration)
        {
            configuration["ConnectionStrings:Database"] = DatabaseConnectionString;
            configuration["ConnectionStrings:AiJoyTag"] = AiJoyTagConnectionString;
            configuration["CookieExpireTime"] = CookieExpireTime.ToString();
            configuration["PreviewMaxHeight"] = PreviewMaxHeight.ToString();
            configuration["TaskSchedulerWorkerCount"] = TaskSchedulerWorkerCount.ToString();
            configuration["TaskSchedulerAutotaggingWorkerCount"] = TaskSchedulerWorkerCount.ToString();
            configuration["PasswordMinLength"] = PasswordMinLength.ToString();
            configuration["PasswordMustHaveSymbols"] = PasswordMustHaveSymbols.ToString();
        }
    }
}
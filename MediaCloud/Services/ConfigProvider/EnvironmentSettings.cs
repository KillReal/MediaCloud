using SixLabors.ImageSharp;

namespace MediaCloud.WebApp.Services.ConfigurationProvider 
{
    [Serializable]
    public class EnvironmentSettings
    {
        public string? DatabaseConnectionString {get; set;}
        public string? PreviewAiAutotagProcessingPath {get; set;}
        public string? AiJoyTagExecutionPath {get; set;}
        public string? AiJoyTagTagsPath {get; set;}
        public string? PythonPath {get; set;}
        public int CookieExpireTime {get; set;}
        public int PreviewMaxHeight {get; set;}
        public int TaskSchedulerWorkerCount {get; set;}
        public int TaskSchedulerAutotaggingWorkerCount {get; set;}
        public int PasswordMinLength {get; set;}
        public bool PasswordMustHaveSymbols {get; set;}

        public EnvironmentSettings(IConfiguration configuration)
        {
            DatabaseConnectionString = configuration?["ConnectionStrings:Database"];
            PreviewAiAutotagProcessingPath = configuration.GetValue<string>("PreviewAiAutotagProcessingPath");
            AiJoyTagExecutionPath = configuration.GetValue<string>("AiJoyTagExecutionPath");
            AiJoyTagTagsPath = configuration.GetValue<string>("AiJoyTagTagsPath");
            PythonPath = configuration.GetValue<string>("PythonPath");
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
            configuration["PreviewAiAutotagProcessingPath"] = PreviewAiAutotagProcessingPath;
            configuration["AiJoyTagExecutionPath"] = AiJoyTagExecutionPath;
            configuration["AiJoyTagTagsPath"] = AiJoyTagTagsPath;
            configuration["PythonPath"] = PythonPath;
            configuration["CookieExpireTime"] = CookieExpireTime.ToString();
            configuration["PreviewMaxHeight"] = PreviewMaxHeight.ToString();
            configuration["TaskSchedulerWorkerCount"] = TaskSchedulerWorkerCount.ToString();
            configuration["TaskSchedulerAutotaggingWorkerCount"] = TaskSchedulerWorkerCount.ToString();
            configuration["PasswordMinLength"] = PasswordMinLength.ToString();
            configuration["PasswordMustHaveSymbols"] = PasswordMustHaveSymbols.ToString();
        }
    }
}
namespace MediaCloud.WebApp.Services.ConfigProvider
{
    [Serializable]
    public class EnvironmentSettings
    {
        public string? AutotaggingServiceConnectionString {get; set;}
        public int CookieExpireTime {get; set;}
        public int TaskSchedulerQueueCleanupTime {get; set;}
        public int PreviewMaxHeight {get; set;}
        public int TaskSchedulerWorkerCount {get; set;}

        public int AutotaggingMaxParallelDegree {get; set;}
        public bool AutotaggingEnabled {get; set;}
        public string? AutotaggingAiModel { get; set; }
        public double AutotaggingAiModelConfidence {get; set;}
        public int UploadingMaxParallelDegree {get; set;}
        public int PasswordMinLength {get; set;}
        public bool PasswordMustHaveSymbols {get; set;}
        public bool LimitLoginAttempts {get; set;}
        public bool UseParallelProcessingForUploading {get; set;}
        public bool UseParallelProcessingForAutotagging {get; set;}
        public long MaxFileSize { get; set; }

        public EnvironmentSettings(IConfiguration configuration)
        {
            AutotaggingServiceConnectionString = configuration["ConnectionStrings:AutotaggingService"];
            AutotaggingAiModel = configuration["Autotagging:AiModel"];
            AutotaggingAiModelConfidence = configuration.GetValue<double>("Autotagging:AiModelConfidence");
            CookieExpireTime = configuration.GetValue<int>("Security:CookieExpireTime");
            PreviewMaxHeight = configuration.GetValue<int>("Uploading:Preview:MaxHeight");
            MaxFileSize = configuration.GetValue<int>("Uploading:MaxFileSize");
            TaskSchedulerWorkerCount = configuration.GetValue<int>("TaskSchedulerWorkerCount");
            AutotaggingMaxParallelDegree = configuration.GetValue<int>("Autotagging:MaxParallelThreadCount");
            UploadingMaxParallelDegree = configuration.GetValue<int>("Uploading:MaxParallelThreadCount");
            PasswordMinLength = configuration.GetValue<int>("Security:PasswordMinLength");
            PasswordMustHaveSymbols = configuration.GetValue<bool>("Security:PasswordMustHaveSymbols");
            UseParallelProcessingForUploading = configuration.GetValue<bool>("Uploading:UseParallelProcessing");
            UseParallelProcessingForAutotagging = configuration.GetValue<bool>("Autotagging:UseParallelProcessing");
            LimitLoginAttempts = configuration.GetValue<bool>("Security:LimitLoginAttempts");
            AutotaggingEnabled = configuration.GetValue<bool>("Autotagging:Enabled");
            TaskSchedulerQueueCleanupTime = configuration.GetValue<int>("TaskSchedulerQueueCleanupTime");
        }

        public EnvironmentSettings()
        {
            
        }

        public void SaveToAppSettings(IConfiguration configuration)
        {
            configuration["ConnectionStrings:AutotaggingService"] = AutotaggingServiceConnectionString;
            configuration["Autotagging:AiModel"] = AutotaggingAiModel;
            configuration["Autotagging:AiModelConfidence"] = AutotaggingAiModelConfidence.ToString();
            configuration["Security:CookieExpireTime"] = CookieExpireTime.ToString();
            configuration["Uploading:Preview:MaxHeight"] = PreviewMaxHeight.ToString();
            configuration["Uploading:MaxFileSize"] = MaxFileSize.ToString();
            configuration["TaskSchedulerWorkerCount"] = TaskSchedulerWorkerCount.ToString();
            configuration["Autotagging:MaxParallelDegree"] = TaskSchedulerWorkerCount.ToString();
            configuration["Security:PasswordMinLength"] = PasswordMinLength.ToString();
            configuration["Security:PasswordMustHaveSymbols"] = PasswordMustHaveSymbols.ToString();
            configuration["Uploading:UseParallelProcessing"] = UseParallelProcessingForUploading.ToString();
            configuration["Autotagging:UseParallelProcessing"] = UseParallelProcessingForAutotagging.ToString();
            configuration["Autotagging:UseParallelProcessing"] = AutotaggingEnabled.ToString();
            configuration["Security:LimitLoginAttempts"] = LimitLoginAttempts.ToString();
            configuration["Uploading:MaxParallelThreadCound"] = UploadingMaxParallelDegree.ToString();
            configuration["TaskSchedulerQueueCleanupTime"] = TaskSchedulerQueueCleanupTime.ToString();
        }
    }
}
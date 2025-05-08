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
        public int AutotaggingRequestTimeout {get; set;}
        public string? AutotaggingAiModel { get; set; }
        public double AutotaggingAiModelConfidence {get; set;}
        public int UploadingMaxParallelDegree {get; set;}
        public int PasswordMinLength {get; set;}
        public bool PasswordMustHaveSymbols {get; set;}
        public bool LimitLoginAttempts {get; set;}
        public bool UseParallelProcessingForUploading {get; set;}
        public bool UseParallelProcessingForAutotagging {get; set;}
        public long MaxFileSize { get; set; }
        
        
        public bool AutorateImages { get; set; }
        public int SmallImageProcessingQuality { get; set; }
        public int SmallImageProcessingLevel { get; set;}
        public int SmallImageSizeLimitKb { get; set; }
        public int ImageProcessingQuality { get; set; }
        public int ImageProcessingLevel { get; set;}
        public int ImageSizeLimitKb { get; set; }
        public int LargeImageProcessingQuality { get; set; }
        public int LargeImageProcessingLevel { get; set;}

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
            AutotaggingRequestTimeout = configuration.GetValue<int>("Autotagging:RequestTimeout");
            TaskSchedulerQueueCleanupTime = configuration.GetValue<int>("TaskSchedulerQueueCleanupTime");
            
            AutorateImages = configuration.GetValue<bool>("Uploading:AutorateImages");
            SmallImageProcessingQuality = configuration.GetValue<int>("Uploading:SmallImageProcessing:Quality");
            SmallImageProcessingLevel = configuration.GetValue<int>("Uploading:SmallImageProcessing:Level");
            SmallImageSizeLimitKb = configuration.GetValue<int>("Uploading:SmallImageProcessing:SizeLimitKb");
            ImageProcessingQuality = configuration.GetValue<int>("Uploading:ImageProcessing:Quality");
            ImageProcessingLevel = configuration.GetValue<int>("Uploading:ImageProcessing:Level");
            ImageSizeLimitKb = configuration.GetValue<int>("Uploading:ImageProcessing:SizeLimitKb");
            LargeImageProcessingQuality = configuration.GetValue<int>("Uploading:LargeImageProcessing:Quality");
            LargeImageProcessingLevel = configuration.GetValue<int>("Uploading:LargeImageProcessing:Level");
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
            configuration["Autotagging:Enabled"] = AutotaggingEnabled.ToString();
            configuration["Autotagging:RequestTimeout"] = AutotaggingRequestTimeout.ToString();
            configuration["Security:LimitLoginAttempts"] = LimitLoginAttempts.ToString();
            configuration["Uploading:MaxParallelThreadCount"] = UploadingMaxParallelDegree.ToString();
            configuration["TaskSchedulerQueueCleanupTime"] = TaskSchedulerQueueCleanupTime.ToString();
            
            configuration["Uploading:AutorateImages"] = AutorateImages.ToString();
            configuration["Uploading:SmallImageProcessing:Quality"] = SmallImageProcessingQuality.ToString();
            configuration["Uploading:SmallImageProcessing:Level"] = SmallImageProcessingLevel.ToString();
            configuration["Uploading:SmallImageProcessing:SizeLimitKb"] = SmallImageSizeLimitKb.ToString();
            configuration["Uploading:ImageProcessing:Quality"] = ImageProcessingQuality.ToString();
            configuration["Uploading:ImageProcessing:Level"] = ImageProcessingLevel.ToString();
            configuration["Uploading:ImageProcessing:SizeLimitKb"] = ImageSizeLimitKb.ToString();
            configuration["Uploading:LargeImageProcessing:Quality"] = LargeImageProcessingQuality.ToString();
            configuration["Uploading:LargeImageProcessing:Level"] = LargeImageProcessingLevel.ToString();
        }
    }
}
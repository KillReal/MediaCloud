namespace MediaCloud.WebApp.Services.ConfigProvider 
{
    [Serializable]
    public class UserSettings
    {
        public int ListMaxEntitiesCount { get; set; }
        public int ListMaxPageCount { get; set; }
        public int StatisticActivityBacktrackDayCount { get; set; }
        public bool ListAutoloadingEnabled { get; set; }

        public UserSettings(IConfiguration configuration)
        {   
            ListMaxEntitiesCount = configuration.GetValue<int>("Gallery:List:MaxEntitiesCount");
            ListMaxPageCount = configuration.GetValue<int>("Gallery:List:MaxPageCount");
            ListAutoloadingEnabled = configuration.GetValue<bool>("Gallery:List:AutoloadingEnabled");
            StatisticActivityBacktrackDayCount = configuration.GetValue<int>("StatisticActivityBacktrackDayCount");
        }

        public UserSettings()
        {

        }
    }
}
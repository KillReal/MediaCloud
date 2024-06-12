namespace MediaCloud.WebApp.Services.ConfigurationProvider 
{
    [Serializable]
    public class ActorSettings
    {
        public int ListMaxEntitiesCount { get; set; }
        public int ListMaxPageCount { get; set; }
        public int StatisticActivityBacktrackDayCount { get; set; }
        public bool ListAutoloadingEnabled { get; set; }

        public ActorSettings(IConfiguration configuration)
        {   
            ListMaxEntitiesCount = configuration.GetValue<int>("ListMaxEntitiesCount");
            ListMaxPageCount = configuration.GetValue<int>("ListMaxPageCount");
            ListAutoloadingEnabled = configuration.GetValue<bool>("ListAutoloadingEnabled");
            StatisticActivityBacktrackDayCount = configuration.GetValue<int>("StatisticActivityBacktrackDayCount");
        }

        public ActorSettings()
        {

        }
    }
}
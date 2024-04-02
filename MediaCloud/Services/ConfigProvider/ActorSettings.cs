namespace MediaCloud.WebApp.Services.ConfigurationProvider 
{
    [Serializable]
    public class ActorSettings
    {
        public int ListMaxEntitiesCount { get; set; }
        public int ListMaxPageCount { get; set; }
        public int GalleryMaxColumnCount { get; set; }
        public int StatisticActivityBacktrackDayCount { get; set; }

        public ActorSettings(IConfiguration configuration)
        {   
            ListMaxEntitiesCount = configuration.GetValue<int>("ListMaxEntitiesCount");
            ListMaxPageCount = configuration.GetValue<int>("ListMaxPageCount");
            GalleryMaxColumnCount = configuration.GetValue<int>("GalleryMaxColumnCount");
            StatisticActivityBacktrackDayCount = configuration.GetValue<int>("StatisticActivityBacktrackDayCount");
        }

        public ActorSettings()
        {

        }
    }
}
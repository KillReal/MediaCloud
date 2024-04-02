namespace MediaCloud.WebApp.Services.ConfigurationProvider 
{
    [Serializable]
    public class ActorSettings
    {
        public int CookieExpireTime { get; set; }
        public int PreviewMaxHeight { get; set; }
        public int ListMaxEntitiesCount { get; set; }
        public int ListMaxPageCount { get; set; }
        public int GalleryMaxColumnCount { get; set; }
        public int StatisticActivityBacktrackDayCount { get; set; }

        public ActorSettings(IConfiguration configuration)
        {   
            CookieExpireTime = configuration.GetValue<int>("CookieExpireTime");
            PreviewMaxHeight = configuration.GetValue<int>("PreviewMaxHeight");
            ListMaxEntitiesCount = configuration.GetValue<int>("ListMaxEntitiesCount");
            ListMaxPageCount = configuration.GetValue<int>("ListMaxPageCount");
            GalleryMaxColumnCount = configuration.GetValue<int>("GalleryMaxColumnCount");
            StatisticActivityBacktrackDayCount = configuration.GetValue<int>("StatisticActivityBacktrackDayCount");
        }
    }
}
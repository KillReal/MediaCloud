using System.Globalization;

namespace MediaCloud.WebApp.Services
{
    public static class ConfigurationService
    {
        private static IConfiguration Configuration;

        public static void Init(IConfiguration configuration)
            => Configuration = configuration;

        public static class Preview
        {
            public static int GetMaxHeight()
                => Convert.ToInt32(Configuration["MaxPreviewHeight"]);
        }

        public static class List
        {
            public static int GetEntityMaxCount()
                => Convert.ToInt32(Configuration["ListEntityCount"]);

            public static int GetShowedPagesMaxCount()
                => Convert.ToInt32(Configuration["MaxShowedPages"]);
        }

        public static class Gallery
        {
            public static int GetColumnCount()
                => Convert.ToInt32(Configuration["GalleryColumnCount"]);
        }

        public static class Statistic
        {
            public static double GetSizeTargetError()
                => double.Parse(Configuration["StatisticMediasSizeTargetError"], CultureInfo.InvariantCulture);

            public static int GetActivityBacktrackDayCount()
                => Convert.ToInt32(Configuration["StatisticActivityBacktrackDayCount"]);
        }
    }
}

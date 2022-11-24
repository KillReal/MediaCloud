namespace MediaCloud.WebApp.Services
{
    public static class ConfigurationService
    {
        private static IConfiguration _configuration;

        public static void LazyInit(IConfiguration configuration)
            => _configuration = configuration;

        public static class Preview
        {
            public static int GetMaxHeight()
                => Convert.ToInt32(_configuration["MaxPreviewHeight"]);
        }

        public static class List
        {
            public static int GetEntityMaxCount()
                => Convert.ToInt32(_configuration["ListEntityCount"]);

            public static int GetShowedPagesMaxCount()
                => Convert.ToInt32(_configuration["MaxShowedPages"]);
        }

        public static class Gallery
        {
            public static int GetColumnCount()
                => Convert.ToInt32(_configuration["GalleryColumnCount"]);
        }
    }
}

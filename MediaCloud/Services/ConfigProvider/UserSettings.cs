using System.ComponentModel.DataAnnotations;
using MediaCloud.WebApp.Data.Types;

namespace MediaCloud.WebApp.Services.ConfigProvider 
{
    [Serializable]
    public class UserSettings
    {
        [Required]
        [AllowedValues(12, 24, 36, 48, 64)]
        public int ListMaxEntitiesCount { get; set; }
        
        [Required]
        [AllowedValues(4, 6, 8, 10, 12, 16)]
        public int ListMaxPageCount { get; set; }
        
        [Required]
        [AllowedValues(7, 14, 30, 60, 90)]
        public int StatisticActivityBacktrackDayCount { get; set; }
        [Required]
        [AllowedValues(15, 30, 50, 100, 200, 300)]
        public int StatisticTagsTopCount { get; set; }
        [Required]
        [AllowedValues(2, 3, 4, 5, 6)]
        public int MaxColumnsCount { get; set; }
        
        [Required]
        public bool ListAutoloadingEnabled { get; set; }
        [Required]
        [AllowedValues("Light", "Dark")]
        public string UITheme { get; set; }
        [Required]
        [AllowedValues(PreviewRatingType.Unknown, PreviewRatingType.General, PreviewRatingType.Sensitive, PreviewRatingType.Questionable, PreviewRatingType.Explicit)]
        public PreviewRatingType AllowedNSFWContent  { get; set; }

        public UserSettings(IConfiguration configuration)
        {   
            ListMaxEntitiesCount = configuration.GetValue<int>("Gallery:List:MaxEntitiesCount");
            ListMaxPageCount = configuration.GetValue<int>("Gallery:List:MaxPageCount");
            ListAutoloadingEnabled = configuration.GetValue<bool>("Gallery:List:AutoloadingEnabled");
            StatisticActivityBacktrackDayCount = configuration.GetValue<int>("StatisticActivityBacktrackDayCount");
            StatisticTagsTopCount = configuration.GetValue<int>("StatisticTagsTopCount");
            MaxColumnsCount = configuration.GetValue<int>("Gallery:MaxColumnCount");
            UITheme = configuration.GetValue<string>("UI:Theme") ?? "Light";
            AllowedNSFWContent = configuration.GetValue<PreviewRatingType>("Gallery:AllowedNSFWContent");
        }

        public UserSettings()
        {

        }
    }
}
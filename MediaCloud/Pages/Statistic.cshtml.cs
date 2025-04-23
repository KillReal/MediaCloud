using MediaCloud.Builders.List;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.Pages
{
    public class StatisticModel(IUserProvider userProvider, IConfigProvider configProvider, StatisticProvider statisticProvider,
        TagRepository tagRepository) : AuthorizedPageModel(userProvider, configProvider)
    {
        private readonly IConfigProvider _configProvider = configProvider;

        [BindProperty]
        public int ActivityBacktrackDayCount { get; set; }
        [BindProperty]
        public List<StatisticSnapshot> Snapshots { get; set; } = [];
        [BindProperty]
        public List<Tag> Tags { get; set; } = [];
        [BindProperty]
        public int TotalTagsCount { get; set; }

        public async Task<IActionResult> OnGet(int limit = 20)
        {
            ActivityBacktrackDayCount = _configProvider.UserSettings.StatisticActivityBacktrackDayCount;
            Snapshots = statisticProvider.GetAllSnapshots();
            Tags = tagRepository.GetTopUsed(limit).Where(x => x.PreviewsCount > 0).ToList();
            TotalTagsCount = await tagRepository.GetTotalCountAsync();

            return Page();
        }
    }
}
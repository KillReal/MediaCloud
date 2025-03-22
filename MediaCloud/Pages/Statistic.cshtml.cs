using MediaCloud.Data;
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
        TagRepository tagRepository, AppDbContext context) : AuthorizedPageModel(userProvider, configProvider)
    {
        private readonly StatisticProvider _statisticProvider = statisticProvider;
        private readonly IConfigProvider _configProvider = configProvider;
        private readonly TagRepository _tagRepository = tagRepository;
        private readonly AppDbContext _context = context;

        [BindProperty]
        public int ActivityBacktrackDayCount { get; set; }
        [BindProperty]
        public List<StatisticSnapshot> Snapshots { get; set; } = [];
        [BindProperty]
        public List<Tag> Tags { get; set; } = [];

        public IActionResult OnGet()
        {
            ActivityBacktrackDayCount = _configProvider.UserSettings.StatisticActivityBacktrackDayCount;
            Snapshots = _statisticProvider.GetAllSnapshots();
            Tags = _tagRepository.GetTopUsed(15).Where(x => x.PreviewsCount > 0).ToList();

            var actualSize = _context.GetDbSize();
            var aproximateSize = Snapshots.Last().MediasSize;

            return Page();
        }
    }
}
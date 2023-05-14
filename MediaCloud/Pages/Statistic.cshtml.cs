using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services;
using MediaCloud.WebApp.Services.Repository;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediaCloud.Pages
{
    public class StatisticModel : PageModel
    {
        private readonly ILogger<PrivacyModel> Logger;
        private readonly IStatisticService StatisticService;
        private readonly IRepository Repository;

        [BindProperty]
        public double SizeTargetError { get; set; }
        [BindProperty]
        public int ActivityBacktrackDayCount { get; set; }
        [BindProperty]
        public List<StatisticSnapshot> Snapshots { get; set; }
        [BindProperty]
        public List<Tag> Tags { get; set; } 

        public StatisticModel(ILogger<PrivacyModel> logger, IStatisticService statisticService, IRepository repository)
        {
            Logger = logger;
            StatisticService = statisticService;
            Repository = repository;
        }

        public IActionResult OnGet()
        {
            ActivityBacktrackDayCount = ConfigurationService.Statistic.GetActivityBacktrackDayCount();
            Snapshots = StatisticService.GetStatistic();
            Tags = Repository.Tags.GetTopUsed(15);

            var path = ConfigurationService.Database.GetPath();
            var actualSize = new FileInfo(path).Length;
            var aproximateSize = Snapshots.Last().MediasSize;

            SizeTargetError = Math.Round((double)((actualSize - aproximateSize) / (double)aproximateSize), 3);

            return Page();
        }
    }
}
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services;
using MediaCloud.WebApp.Services.DataService;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediaCloud.Pages
{
    public class StatisticModel : PageModel
    {
        private readonly IStatisticService _statisticService;
        private readonly IDataService _dataService;

        [BindProperty]
        public double SizeTargetError { get; set; }
        [BindProperty]
        public int ActivityBacktrackDayCount { get; set; }
        [BindProperty]
        public List<StatisticSnapshot> Snapshots { get; set; } = new();
        [BindProperty]
        public List<Tag> Tags { get; set; } = new();

        public StatisticModel(IStatisticService statisticService, IDataService dataService)
        {
            _statisticService = statisticService;
            _dataService = dataService;
        }

        public IActionResult OnGet()
        {
            ActivityBacktrackDayCount = ConfigurationService.Statistic.GetActivityBacktrackDayCount();
            Snapshots = _statisticService.GetStatistic();
            Tags = _dataService.Tags.GetTopUsed(15).Where(x => x.PreviewsCount > 0).ToList();

            var actualSize = _dataService.GetDbSize();
            var aproximateSize = Snapshots.Last().MediasSize;

            SizeTargetError = Math.Round((double)((actualSize - aproximateSize) / (double)aproximateSize), 3);

            return Page();
        }
    }
}
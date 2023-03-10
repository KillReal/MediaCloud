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
        private readonly ILogger<PrivacyModel> _logger;
        private readonly IStatisticService _statisticService;
        private readonly IRepository _repository;

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
            _logger = logger;
            _statisticService = statisticService;
            _repository = repository;
        }

        public IActionResult OnGet()
        {
            SizeTargetError = ConfigurationService.Statistic.GetSizeTargetError();
            ActivityBacktrackDayCount = ConfigurationService.Statistic.GetActivityBacktrackDayCount();
            Snapshots = _statisticService.GetStatistic();
            Tags = _repository.Tags.GetTopUsed(15);
            _logger.LogInformation($"Loaded {Snapshots.Count} snapshots");

            return Page();
        }
    }
}
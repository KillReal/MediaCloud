using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services;
using MediaCloud.WebApp.Services.ActorProvider;
using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediaCloud.Pages
{
    public class StatisticModel : AuthorizedPageModel
    {
        private readonly StatisticProvider _statisticProvider;
        private readonly IConfigProvider _configProvider;
        private readonly TagRepository _tagRepository;
        private readonly AppDbContext _context;

        [BindProperty]
        public double SizeTargetError { get; set; }
        [BindProperty]
        public int ActivityBacktrackDayCount { get; set; }
        [BindProperty]
        public List<StatisticSnapshot> Snapshots { get; set; } = new();
        [BindProperty]
        public List<Tag> Tags { get; set; } = new();

        public StatisticModel(IActorProvider actorProvider, IConfigProvider configProvider, StatisticProvider statisticProvider,
            TagRepository tagRepository, AppDbContext context) 
            : base(actorProvider)
        {
            _statisticProvider = statisticProvider;
            _configProvider = configProvider;
            _tagRepository = tagRepository;
            _context = context;
        }

        public IActionResult OnGet()
        {
            ActivityBacktrackDayCount = _configProvider.ActorSettings.StatisticActivityBacktrackDayCount;
            Snapshots = _statisticProvider.GetAllSnapshots();
            Tags = _tagRepository.GetTopUsed(15).Where(x => x.PreviewsCount > 0).ToList();

            var actualSize = _context.GetDbSize();
            var aproximateSize = Snapshots.Last().MediasSize;

            SizeTargetError = (actualSize - aproximateSize) / (double)aproximateSize;

            return Page();
        }
    }
}
﻿using MediaCloud.Data.Models;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services;
using MediaCloud.WebApp.Services.DataService;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediaCloud.Pages
{
    public class StatisticModel : BasePageModel
    {
        private readonly IStatisticService _statisticService;

        [BindProperty]
        public double SizeTargetError { get; set; }
        [BindProperty]
        public int ActivityBacktrackDayCount { get; set; }
        [BindProperty]
        public List<StatisticSnapshot> Snapshots { get; set; } = new();
        [BindProperty]
        public List<Tag> Tags { get; set; } = new();

        public StatisticModel(IStatisticService statisticService, IDataService dataService) : base(dataService)
        {
            _statisticService = statisticService;
        }

        public IActionResult OnGet()
        {
            ActivityBacktrackDayCount = ConfigurationService.Statistic.GetActivityBacktrackDayCount();
            Snapshots = _statisticService.GetStatistic();
            Tags = _dataService.Tags.GetTopUsed(15).Where(x => x.PreviewsCount > 0).ToList();

            var actualSize = _dataService.GetDbSize();
            var aproximateSize = Snapshots.Last().MediasSize;

            SizeTargetError = (actualSize - aproximateSize) / (double)aproximateSize;

            return Page();
        }
    }
}
using MediaCloud.Data.Models;
using MediaCloud.Extensions;
using MediaCloud.MediaUploader;
using MediaCloud.Pages.Actors;
using MediaCloud.Services;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.WebApp.Controllers
{
    public class StatisticController : Controller
    {
        private IStatisticService StatisticService;

        public IActionResult Index()
        {
            return View();
        }

        public StatisticController(IStatisticService statisticService)
        {
            StatisticService = statisticService;
        }

        public dynamic GetCurrent()
        {
            var snapshot = StatisticService.GetTodayStatistic();
            return new
            {
                Status = StatisticService.GetStatus().GetDisplayName(),
                ActorsCount = snapshot.ActorsCount,
                TagsCount = snapshot.TagsCount,
                MediasCount = snapshot.MediasCount,
                MediasSize = PictureService.FormatSize(snapshot.MediasSize),
                ActivityFactor = snapshot.ActivityFactor
            };
        }

        public void Recalculate()
        {
            StatisticService.ProceedRecalculaton();
        }
    }
}

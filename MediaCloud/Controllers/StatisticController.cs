using MediaCloud.Data.Models;
using MediaCloud.Extensions;
using MediaCloud.MediaUploader;
using MediaCloud.Pages.Actors;
using MediaCloud.Services;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.WebApp.Controllers
{
    [Authorize]
    public class StatisticController : Controller
    {
        private readonly IStatisticService _statisticService;

        public IActionResult Index()
        {
            return View();
        }

        public StatisticController(IStatisticService statisticService)
        {
            _statisticService = statisticService;
        }

        public dynamic GetCurrent()
        {
            var snapshot = _statisticService.GetTodayStatistic();
            return new
            {
                Status = _statisticService.GetStatus().GetDisplayName(),
                snapshot.ActorsCount,
                snapshot.TagsCount,
                snapshot.MediasCount,
                MediasSize = PictureService.FormatSize(snapshot.MediasSize),
                snapshot.ActivityFactor
            };
        }

        public void Recalculate(int days = 0)
        {
            if (days != 0)
            {
                _statisticService.ProceedRecalculaton(days);
                return;
            }

            _statisticService.ProceedRecalculaton();
        }

        public string Status()
        {
            return _statisticService.GetStatus().GetDisplayName();
        }
    }
}

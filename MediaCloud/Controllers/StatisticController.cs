using MediaCloud.Data.Models;
using MediaCloud.Extensions;
using MediaCloud.MediaUploader;
using MediaCloud.Pages.Actors;
using MediaCloud.Services;
using MediaCloud.WebApp.Services.DataService;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.WebApp.Controllers
{
    [Authorize]
    public class StatisticController : Controller
    {
        private readonly StatisticProvider _statisticProvider;
        private readonly IUploader _uploader;

        public IActionResult Index()
        {
            return View();
        }

        public StatisticController(IDataService dataService, IUploader uploader)
        {
            _statisticProvider = dataService.StatisticProvider;
            _uploader = uploader;
        }

        public dynamic GetCurrent()
        {
            var snapshot = _statisticProvider.GetTodaySnapshot();

            return new
            {
                snapshot.ActorsCount,
                snapshot.TagsCount,
                snapshot.MediasCount,
                MediasSize = PictureService.FormatSize(snapshot.MediasSize),
                snapshot.ActivityFactor
            };
        }

        public IActionResult Recalculate(int days = 0)
        {
            _statisticProvider.RemoveAllSnapshots();
            var task = _statisticProvider.GetRecalculationTask(days);

            _uploader.AddTask(task);

            return Redirect($"/Uploader/GetTaskStatus?id={task.Id}");
        }
    }
}

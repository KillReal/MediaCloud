using MediaCloud.Extensions;
using MediaCloud.TaskScheduler;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.WebApp.Controllers
{
    [Authorize]
    public class StatisticController(StatisticProvider statisticProvider, ITaskScheduler uploader) : Controller
    {
        private readonly StatisticProvider _statisticProvider = statisticProvider;
        private readonly ITaskScheduler _uploader = uploader;

        public IActionResult Index()
        {
            return View();
        }

        public dynamic GetCurrent()
        {
            var snapshot = _statisticProvider.GetTodaySnapshot();

            return new
            {
                snapshot.ActorsCount,
                snapshot.TagsCount,
                snapshot.MediasCount,
                MediasSize = snapshot.MediasSize.FormatSize(),
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

    public interface IStatisticProvider
    {
    }
}

using MediaCloud.Extensions;
using MediaCloud.TaskScheduler;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.WebApp.Controllers
{
    [Authorize]
    public class StatisticController : Controller
    {
        private readonly StatisticProvider _statisticProvider;
        private readonly ITaskScheduler _uploader;

        public IActionResult Index()
        {
            return View();
        }

        public StatisticController(StatisticProvider statisticProvider, ITaskScheduler uploader)
        {
            _statisticProvider = statisticProvider;
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

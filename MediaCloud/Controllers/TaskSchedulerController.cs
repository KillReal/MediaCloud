using MediaCloud.TaskScheduler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.WebApp.Controllers
{
    [Authorize]
    public class TaskSchedulerController : Controller
    {
        private readonly ITaskScheduler _taskScheduler;

        public IActionResult Index()
        {
            return View();
        }

        public TaskSchedulerController(ITaskScheduler taskScheduler)
        {
            _taskScheduler = taskScheduler;
        }

        public TaskScheduler.TaskStatus GetTaskStatus(Guid id)
        {
            return _taskScheduler.GetStatus(id);
        }

        public TaskSchedulerStatus GetStatus()
        {
            return _taskScheduler.GetStatus();
        }
    }
}

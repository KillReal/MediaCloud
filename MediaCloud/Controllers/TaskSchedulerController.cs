using MediaCloud.TaskScheduler;
using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.WebApp.Controllers
{
    public class TaskSchedulerController(ITaskScheduler taskScheduler) : Controller
    {
        private readonly ITaskScheduler _taskScheduler = taskScheduler;

        public IActionResult Index()
        {
            return View();
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

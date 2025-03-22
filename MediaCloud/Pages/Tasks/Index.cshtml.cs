using MediaCloud.TaskScheduler;
using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp.Services.UserProvider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediaCloud.WebApp.Pages.Tasks
{
    public class ListModel(IUserProvider userProvider, ITaskScheduler taskScheduler, IConfigProvider configProvider) 
        : AuthorizedPageModel(userProvider, configProvider)
    {
        private readonly ITaskScheduler _taskScheduler = taskScheduler;

        [BindProperty]
        public TaskSchedulerStatus TaskSchedulerStatus { get; set; }

        public IActionResult OnGet()
        {
            TaskSchedulerStatus = _taskScheduler.GetStatus();

            if (CurrentUser == null || CurrentUser.IsAdmin == false)
            {
                return Redirect("/Error");
            }

            return Page();
        }

        public IActionResult OnGetCleanupCompleted()
        {
            if (CurrentUser == null || CurrentUser.IsAdmin == false)
            {
                return Redirect("/Error");
            }

            _taskScheduler.CleanupQueue();

            return Redirect("/Tags");
        }

        public IActionResult OnGetCleanupAll()
        {
            if (CurrentUser == null || CurrentUser.IsAdmin == false)
            {
                return Redirect("/Error");
            }

            _taskScheduler.CleanupQueue(false);

            return Redirect("/Tags");
        }
    }
}
